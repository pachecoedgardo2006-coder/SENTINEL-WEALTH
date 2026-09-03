namespace SentinelWealth.Api.Application.DTOs;

public sealed class PortfolioSummaryDto
{
    public required decimal TotalNetWorth { get; init; }
    public required string Currency { get; init; }
    public required string RiskScore { get; init; }
    public required int RiskNumeric { get; init; }
    public required string ClientName { get; init; }
    public required string Tier { get; init; }
    public required string Domicile { get; init; }
    public required string ShieldStatus { get; init; }
    public required IReadOnlyList<AssetDto> Assets { get; init; }
    public required IReadOnlyList<AlertDto> RecentAlerts { get; init; }
}

public sealed class AssetDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Class { get; init; }
    public required decimal Value { get; init; }
    public required decimal AllocationPercent { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public string? Ticker { get; init; }
    public decimal? DayChangePercent { get; init; }
    public string? Custodian { get; init; }
}

public sealed class AlertDto
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required string Severity { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }
    public string? AnalyzedBy { get; init; }
    public decimal? AmountUsd { get; init; }
    public string? SourceIp { get; init; }
    public string? RecommendedAction { get; init; }
}
