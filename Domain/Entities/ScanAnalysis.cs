namespace SentinelWealth.Api.Domain.Entities;

public sealed class ScanAnalysis
{
    public required int ThreatScore { get; init; }
    public required string Verdict { get; init; }
    public required string GrokAnalysisSummary { get; init; }
    public required IReadOnlyList<string> Flags { get; init; }
    public required string RecommendedAction { get; init; }
    public required string Engine { get; init; }
}
