using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Infrastructure.Grok;

public sealed class GrokApiClient : IGrokApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrokApiClient> _logger;
    private readonly HeuristicFraudAnalyzer _heuristic;

    public GrokApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GrokApiClient> logger,
        HeuristicFraudAnalyzer heuristic)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _heuristic = heuristic;
    }

    public async Task<ScanAnalysis> AnalyzeAsync(
        string inputType,
        string content,
        CancellationToken cancellationToken)
    {
        var provider = ResolveProvider();
        if (provider is null)
        {
            _logger.LogInformation("Sin API key (GEMINI_API_KEY / XAI_API_KEY). Fallback heurístico.");
            return _heuristic.Analyze(inputType, content);
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ResolveTimeoutSeconds()));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        try
        {
            ScanAnalysis? live = provider.Name == "gemini"
                ? await CallGeminiAsync(provider, inputType, content, linked.Token)
                : await CallOpenAiCompatAsync(provider, inputType, content, linked.Token);

            if (live is not null)
            {
                return live;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("{Provider} superó el umbral de {Seconds}s. Fallback heurístico.", provider.Name, ResolveTimeoutSeconds());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo llamando a {Provider}. Fallback heurístico.", provider.Name);
        }

        return _heuristic.Analyze(inputType, content);
    }

    private async Task<ScanAnalysis?> CallGeminiAsync(
        LlmProvider provider,
        string inputType,
        string content,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(inputType, content);
        var url = $"{provider.BaseUrl}/models/{provider.Model}:generateContent";
        var payload = JsonSerializer.Serialize(new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = "Eres el analista de fraude patrimonial de SentinelWealth AI. " +
                               "Responde SOLO JSON válido, sin markdown."
                    }
                }
            },
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("x-goog-api-key", provider.ApiKey);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gemini {Url} respondió {Status}: {Body}", url, (int)response.StatusCode, Truncate(errorBody));
            return await CallOpenAiCompatAsync(provider with
            {
                BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai"
            }, inputType, content, cancellationToken);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelText = ExtractModelText(body);
        return string.IsNullOrWhiteSpace(modelText) ? null : ParseAnalysis(modelText, "gemini");
    }

    private async Task<ScanAnalysis?> CallOpenAiCompatAsync(
        LlmProvider provider,
        string inputType,
        string content,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(inputType, content);

        var chatPayload = JsonSerializer.Serialize(new
        {
            model = provider.Model,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Eres el analista de fraude patrimonial de SentinelWealth AI. " +
                              "Responde SOLO JSON válido, sin markdown."
                },
                new { role = "user", content = prompt }
            }
        });

        var chatResult = await PostAndParseAsync(
            $"{provider.BaseUrl}/chat/completions",
            provider.ApiKey,
            chatPayload,
            provider.Name,
            cancellationToken);

        if (chatResult is not null)
        {
            return chatResult;
        }

        if (provider.Name == "gemini")
        {
            return null;
        }

        var responsesPayload = JsonSerializer.Serialize(new
        {
            model = provider.Model,
            store = false,
            input = prompt
        });

        return await PostAndParseAsync(
            $"{provider.BaseUrl}/responses",
            provider.ApiKey,
            responsesPayload,
            provider.Name,
            cancellationToken);
    }

    private async Task<ScanAnalysis?> PostAndParseAsync(
        string url,
        string apiKey,
        string payload,
        string engine,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("{Engine} {Url} respondió {Status}", engine, url, (int)response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelText = ExtractModelText(body);
        if (string.IsNullOrWhiteSpace(modelText))
        {
            return null;
        }

        return ParseAnalysis(modelText, engine);
    }

    private static string ExtractModelText(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("choices", out var choices)
            && choices.ValueKind == JsonValueKind.Array
            && choices.GetArrayLength() > 0)
        {
            var message = choices[0].GetProperty("message");
            if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
            {
                return content.GetString() ?? string.Empty;
            }
        }

        if (root.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == JsonValueKind.Array
            && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var geminiContent)
                && geminiContent.TryGetProperty("parts", out var parts)
                && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    {
                        return text.GetString() ?? string.Empty;
                    }
                }
            }
        }

        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    {
                        return text.GetString() ?? string.Empty;
                    }
                }
            }
        }

        return string.Empty;
    }

    private ScanAnalysis? ParseAnalysis(string modelText, string engine)
    {
        var json = UnwrapJson(modelText);
        GrokStructuredResult? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GrokStructuredResult>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON del modelo no parseable.");
            return null;
        }

        if (parsed is null || parsed.ThreatScore is null || string.IsNullOrWhiteSpace(parsed.Verdict))
        {
            return null;
        }

        return new ScanAnalysis
        {
            ThreatScore = Math.Clamp(parsed.ThreatScore.Value, 0, 100),
            Verdict = parsed.Verdict,
            GrokAnalysisSummary = parsed.GrokAnalysisSummary ?? parsed.Verdict,
            Flags = parsed.Flags is { Count: > 0 } ? parsed.Flags : ["Análisis en vivo sin flags adicionales"],
            RecommendedAction = parsed.RecommendedAction ?? "Verificar el origen por un canal oficial.",
            Engine = engine
        };
    }

    private static string UnwrapJson(string modelText)
    {
        var trimmed = modelText.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBreak = trimmed.IndexOf('\n');
            if (firstBreak > 0)
            {
                trimmed = trimmed[(firstBreak + 1)..];
            }

            var fence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fence >= 0)
            {
                trimmed = trimmed[..fence];
            }
        }

        return trimmed.Trim();
    }

    private static string BuildPrompt(string inputType, string content)
    {
        const string schema = """
            {
              "threatScore": <entero 0-100>,
              "verdict": "<veredicto corto en español, ej. Alto Riesgo — ...>",
              "grokAnalysisSummary": "<2-4 frases técnicas en español>",
              "flags": ["<señal 1>", "<señal 2>"],
              "recommendedAction": "<acción concreta en español>"
            }
            """;

        return
            "Analiza este posible fraude financiero para SentinelWealth AI (HNW + escáner ciudadano).\n" +
            $"inputType: {inputType}\n" +
            $"content:\n{content}\n\n" +
            "Devuelve EXACTAMENTE este JSON:\n" +
            schema;
    }

    private LlmProvider? ResolveProvider()
    {
        var configured = _configuration["Grok:ApiKey"];
        var gemini = FirstNonEmpty(
            Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
            Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
            _configuration["Grok:GeminiApiKey"]);
        var xai = FirstNonEmpty(Environment.GetEnvironmentVariable("XAI_API_KEY"));

        if (LooksLikeGeminiKey(configured))
        {
            gemini ??= configured;
        }
        else if (!string.IsNullOrWhiteSpace(configured))
        {
            xai ??= configured;
        }

        var preferred = (_configuration["Grok:Provider"] ?? "auto").Trim().ToLowerInvariant();
        var useGemini = preferred is "gemini"
            || (preferred == "auto" && !string.IsNullOrWhiteSpace(gemini));

        if (useGemini)
        {
            var key = FirstNonEmpty(gemini, configured);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var model = FirstNonEmpty(_configuration["Grok:Model"], "gemini-3.5-flash-lite")!;
            var baseUrl = FirstNonEmpty(
                _configuration["Grok:BaseUrl"],
                "https://generativelanguage.googleapis.com/v1beta")!.TrimEnd('/');
            return new LlmProvider("gemini", key, baseUrl, model);
        }

        if (string.IsNullOrWhiteSpace(xai))
        {
            return null;
        }

        var xaiModel = FirstNonEmpty(_configuration["Grok:Model"], "grok-4.6")!;
        var xaiBase = FirstNonEmpty(_configuration["Grok:BaseUrl"], "https://api.x.ai/v1")!.TrimEnd('/');
        return new LlmProvider("grok", xai, xaiBase, xaiModel);
    }

    private static bool LooksLikeGeminiKey(string? key) =>
        !string.IsNullOrWhiteSpace(key)
        && (key.StartsWith("AIza", StringComparison.Ordinal)
            || key.StartsWith("AQ.", StringComparison.Ordinal));

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string Truncate(string value) =>
        value.Length <= 300 ? value : value[..300];

    private int ResolveTimeoutSeconds()
    {
        if (int.TryParse(_configuration["Grok:TimeoutSeconds"], out var seconds) && seconds > 0)
        {
            return seconds;
        }

        return 20;
    }

    private sealed record LlmProvider(string Name, string ApiKey, string BaseUrl, string Model);

    private sealed class GrokStructuredResult
    {
        public int? ThreatScore { get; set; }
        public string? Verdict { get; set; }
        public string? GrokAnalysisSummary { get; set; }
        public List<string>? Flags { get; set; }
        public string? RecommendedAction { get; set; }
    }
}
