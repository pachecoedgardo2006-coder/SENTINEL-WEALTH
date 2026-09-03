using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Infrastructure.Grok;

/// <summary>
/// Analizador heurístico de respaldo. Garantiza una respuesta estructurada
/// creíble si Grok no está disponible, no hay API key o supera 3s.
/// </summary>
public sealed class HeuristicFraudAnalyzer
{
    private static readonly (string Keyword, int Weight, string Flag)[] Signals =
    [
        ("urgente", 18, "Urgencia artificial — clásico de ingeniería social"),
        ("urgent", 18, "Urgencia artificial — clásico de ingeniería social"),
        ("inmediatamente", 14, "Presión temporal para saltar controles"),
        ("transferir", 16, "Solicitud de transferencia de fondos"),
        ("transfer", 16, "Solicitud de transferencia de fondos"),
        ("wire", 18, "Instrucción de wire / SWIFT"),
        ("swift", 16, "Canal SWIFT mencionado fuera de contexto bancario"),
        ("bitcoin", 20, "Pago en cripto — alto riesgo de irreversibilidad"),
        ("btc", 16, "Pago en cripto — alto riesgo de irreversibilidad"),
        ("crypto", 16, "Pago en cripto — alto riesgo de irreversibilidad"),
        ("criptomoneda", 16, "Pago en cripto — alto riesgo de irreversibilidad"),
        ("wallet", 18, "Pide dirección de wallet o semilla"),
        ("billetera", 16, "Pide dirección de wallet o semilla"),
        ("frase semilla", 28, "Intento de robo de seed phrase"),
        ("seed phrase", 28, "Intento de robo de seed phrase"),
        ("12 palabras", 24, "Intento de extraer mnemonic"),
        ("premio", 20, "Cebo de premio / lotería"),
        ("ganaste", 22, "Cebo de premio / lotería"),
        ("lottery", 20, "Cebo de premio / lotería"),
        ("verificar cuenta", 22, "Phishing de credenciales"),
        ("verify your account", 22, "Phishing de credenciales"),
        ("contraseña", 18, "Solicitud de contraseña"),
        ("password", 18, "Solicitud de contraseña"),
        ("otp", 16, "Solicitud de código OTP / 2FA"),
        ("código de verificación", 16, "Solicitud de código OTP / 2FA"),
        ("gift card", 20, "Pago en tarjetas de regalo"),
        ("tarjeta de regalo", 20, "Pago en tarjetas de regalo"),
        ("inversión garantizada", 22, "Promesa de retorno garantizado"),
        ("garantizado", 12, "Promesa de retorno garantizado"),
        ("duplicar", 18, "Esquema de duplicar capital"),
        ("double your", 20, "Esquema de duplicar capital"),
        ("click aquí", 14, "CTA sospechoso"),
        ("haga clic", 14, "CTA sospechoso"),
        ("bit.ly", 16, "Acortador de URL — destino oculto"),
        ("tinyurl", 16, "Acortador de URL — destino oculto"),
        ("cuenta bloqueada", 20, "Pretexto de cuenta bloqueada"),
        ("account suspended", 20, "Pretexto de cuenta bloqueada"),
        ("elon", 14, "Suplantación de figura pública"),
        ("musk", 14, "Suplantación de figura pública"),
        ("whatsapp", 10, "Canal informal de alto abuso"),
        ("telegram", 12, "Canal informal de alto abuso"),
        ("poder irrevocable", 24, "Cláusula de poder irrevocable"),
        ("irrevocable", 16, "Cláusula irrevocable"),
        ("power of attorney", 22, "Cesión de poder / mandato"),
        ("ceder derechos", 18, "Cesión de derechos patrimoniales"),
        ("sin retracto", 16, "Renuncia a retracto"),
        ("kyc urgente", 14, "Falso KYC de emergencia")
    ];

    public ScanAnalysis Analyze(string inputType, string content)
    {
        var text = content ?? string.Empty;
        var normalized = text.ToLowerInvariant();
        var type = (inputType ?? "text").Trim().ToLowerInvariant();

        var flags = new List<string>();
        var score = 12;

        foreach (var (keyword, weight, flag) in Signals)
        {
            if (!normalized.Contains(keyword, StringComparison.Ordinal))
            {
                continue;
            }

            score += weight;
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
            }
        }

        if (type == "url")
        {
            score += 8;
            if (normalized.StartsWith("http://", StringComparison.Ordinal))
            {
                score += 18;
                flags.Add("URL sin TLS (http) — vector típico de phishing");
            }

            if (LooksLikeShortener(normalized) || ContainsSuspiciousTld(normalized))
            {
                score += 16;
                flags.Add("Dominio o acortador con reputación débil");
            }

            if (normalized.Contains("login", StringComparison.Ordinal)
                || normalized.Contains("verify", StringComparison.Ordinal)
                || normalized.Contains("secure-update", StringComparison.Ordinal))
            {
                score += 14;
                flags.Add("Ruta de login/verificación en dominio no oficial");
            }
        }

        if (type == "contract")
        {
            score += 6;
            if (normalized.Contains("firma", StringComparison.Ordinal)
                || normalized.Contains("sign here", StringComparison.Ordinal))
            {
                score += 10;
                flags.Add("Solicita firma inmediata sobre instrumento patrimonial");
            }
        }

        if (LooksLikeUrl(normalized) && type == "text")
        {
            score += 8;
            flags.Add("El mensaje incrusta un enlace — revisar destino real");
        }

        score = Math.Clamp(score, 8, 98);

        if (string.IsNullOrWhiteSpace(text))
        {
            score = 8;
            flags.Clear();
            flags.Add("Entrada vacía — sin señales evaluables");
        }

        if (flags.Count == 0)
        {
            flags.Add("Sin patrones de fraude de alto peso");
            flags.Add("Lenguaje coherente con comunicación legítima");
        }

        var (verdict, action) = ResolveVerdict(score);
        var snippet = string.IsNullOrWhiteSpace(text)
            ? "(sin contenido)"
            : text.Length > 140 ? text[..140] + "…" : text;

        return new ScanAnalysis
        {
            ThreatScore = score,
            Verdict = verdict,
            GrokAnalysisSummary =
                $"Análisis SentinelWealth (motor de respaldo Grok). Tipo={type}. " +
                $"Se evaluaron {Signals.Length} señales lingüísticas y de canal sobre: «{snippet}». " +
                $"Puntaje compuesto {score}/100. " +
                (score >= 60
                    ? "El texto presenta presión, cebo financiero o canal opaco consistentes con estafa dirigida a patrimonio."
                    : "No hay evidencia fuerte de fraude; se recomienda verificación rutinaria del remitente."),
            Flags = flags,
            RecommendedAction = action,
            Engine = "heuristic-fallback"
        };
    }

    private static (string Verdict, string Action) ResolveVerdict(int score) => score switch
    {
        >= 80 => (
            "Alto Riesgo — amenaza crítica de fraude patrimonial",
            "No transferir fondos. Bloquear el canal. Reportar a la entidad financiera y a la Fiscalía / UIAF."),
        >= 60 => (
            "Alto Riesgo — posible estafa financiera",
            "Congelar cualquier instrucción de pago. Verificar identidad por un canal oficial ya conocido."),
        >= 30 => (
            "Riesgo Medio — señales de ingeniería social",
            "No hacer clic ni compartir OTP. Confirmar el origen con el family office o el banco."),
        _ => (
            "Riesgo Bajo — no se detectan patrones de fraude",
            "Monitoreo rutinario. Conservar el mensaje por si el contexto cambia.")
    };

    private static bool LooksLikeUrl(string text) =>
        text.Contains("http://", StringComparison.Ordinal)
        || text.Contains("https://", StringComparison.Ordinal)
        || text.Contains("www.", StringComparison.Ordinal);

    private static bool LooksLikeShortener(string text) =>
        text.Contains("bit.ly", StringComparison.Ordinal)
        || text.Contains("tinyurl", StringComparison.Ordinal)
        || text.Contains("t.co/", StringComparison.Ordinal)
        || text.Contains("rb.gy", StringComparison.Ordinal);

    private static bool ContainsSuspiciousTld(string text) =>
        text.Contains(".xyz", StringComparison.Ordinal)
        || text.Contains(".top", StringComparison.Ordinal)
        || text.Contains(".click", StringComparison.Ordinal)
        || text.Contains(".gq", StringComparison.Ordinal)
        || text.Contains(".tk", StringComparison.Ordinal)
        || text.Contains(".zip", StringComparison.Ordinal);
}
