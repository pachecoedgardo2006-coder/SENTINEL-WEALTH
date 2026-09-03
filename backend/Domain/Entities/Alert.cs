namespace SentinelWealth.Api.Domain.Entities;

public sealed class Alert
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
