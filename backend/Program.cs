using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.Services;
using SentinelWealth.Api.Infrastructure.Grok;
using SentinelWealth.Api.Infrastructure.Persistence;

LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<JsonFileDb>();
builder.Services.AddSingleton<IPortfolioRepository>(sp => sp.GetRequiredService<JsonFileDb>());
builder.Services.AddSingleton<HeuristicFraudAnalyzer>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IFraudScannerService, FraudScannerService>();

builder.Services.AddHttpClient<IGrokApiClient, GrokApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SentinelWealth-AI/1.0");
});

var app = builder.Build();

app.MapOpenApi();
app.UseCors("OpenCors");
app.MapGet("/", () => Results.Ok(new
{
    service = "SentinelWealth.Api",
    docs = "/openapi/v1.json",
    endpoints = new[]
    {
        "POST /api/auth/login",
        "GET /api/health",
        "GET /api/portfolio/summary",
        "POST /api/scanner/analyze"
    }
}));
app.MapControllers();

app.Run();

static void LoadDotEnv(string path)
{
    if (!File.Exists(path))
    {
        return;
    }

    foreach (var raw in File.ReadAllLines(path))
    {
        var line = raw.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            continue;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim().Trim('"').Trim('\'');
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}
