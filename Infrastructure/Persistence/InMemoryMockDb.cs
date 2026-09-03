using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Infrastructure.Persistence;

/// <summary>
/// Store in-memory enriquecido para la demo ante el jurado. Sin EF, sin I/O.
/// </summary>
public sealed class InMemoryMockDb : IPortfolioRepository
{
    private readonly Portfolio _protectedPortfolio;

    public InMemoryMockDb()
    {
        var now = DateTimeOffset.UtcNow;

        _protectedPortfolio = new Portfolio
        {
            Id = "PF-HNW-7741",
            ClientName = "Familia Restrepo-Kaufmann",
            Tier = "HNW / Family Office",
            Domicile = "Barranquilla, Colombia",
            TotalNetWorth = 12_500_000m,
            Currency = "USD",
            RiskScore = "Bajo (18/100)",
            RiskNumeric = 18,
            Assets =
            [
                new Asset
                {
                    Id = "AST-UST-01",
                    Name = "Bonos del Tesoro",
                    Class = "Renta Fija Soberana",
                    Ticker = "UST-10Y / T-Bills",
                    Value = 5_200_000m,
                    AllocationPercent = 41.6m,
                    Currency = "USD",
                    Status = "Protegido",
                    DayChangePercent = 0.12m,
                    Custodian = "JP Morgan Private Bank"
                },
                new Asset
                {
                    Id = "AST-EQ-TECH-02",
                    Name = "Renta Variable Tech",
                    Class = "Equity Growth",
                    Ticker = "NVDA / MSFT / TSLA / xAI SPV",
                    Value = 4_800_000m,
                    AllocationPercent = 38.4m,
                    Currency = "USD",
                    Status = "Protegido — vigilancia de volatilidad",
                    DayChangePercent = -0.84m,
                    Custodian = "Goldman Sachs PWM"
                },
                new Asset
                {
                    Id = "AST-FX-03",
                    Name = "Reservas FX",
                    Class = "Liquidez Multidivisa",
                    Ticker = "USD / EUR / COP",
                    Value = 2_500_000m,
                    AllocationPercent = 20.0m,
                    Currency = "USD",
                    Status = "Protegido — overlay cambiario",
                    DayChangePercent = -1.35m,
                    Custodian = "Bancolombia Banca Privada"
                }
            ],
            RecentAlerts =
            [
                new Alert
                {
                    Id = "ALT-FX-2104",
                    Type = "MarketRisk",
                    Severity = "Media",
                    Title = "Volatilidad cambiaria USD/COP detectada",
                    Description =
                        "Grok identificó un gap intradía de 2.8% en USD/COP que presiona las reservas FX " +
                        "($2.5M). El overlay de cobertura sigue activo; no hay evidencia de ataque, pero " +
                        "la exposición táctica del sleeve de liquidez aumentó el VaR de 24h.",
                    Status = "Activa",
                    DetectedAt = now.AddMinutes(-37),
                    AnalyzedBy = "Grok-4.6 · SentinelWealth Market Shield",
                    AmountUsd = 2_500_000m,
                    RecommendedAction = "Mantener overlay FX y revisar umbral de stop a +3.2%."
                },
                new Alert
                {
                    Id = "ALT-FRD-2105",
                    Type = "Fraud",
                    Severity = "Crítica",
                    Title = "Retiro sospechoso de $180,000 USD bloqueado",
                    Description =
                        "Se interceptó una instrucción de retiro por $180,000 USD hacia una cuenta no " +
                        "whitelistada. La IP de origen (185.220.101.47, ASN Tor-exit / Europa del Este) " +
                        "no coincide con el patrón geográfico del family office (Barranquilla / Miami). " +
                        "SentinelWealth bloqueó el wire SWIFT y pidió step-up MFA.",
                    Status = "Activa — bloqueada",
                    DetectedAt = now.AddMinutes(-12),
                    AnalyzedBy = "SentinelWealth Fraud Engine",
                    AmountUsd = 180_000m,
                    SourceIp = "185.220.101.47",
                    RecommendedAction = "Contactar al titular por canal verificado. No autorizar el SWIFT."
                }
            ]
        };
    }

    public Portfolio GetProtectedPortfolio() => _protectedPortfolio;
}
