namespace SentinelWealth.Api.Application.DTOs;

public sealed class ScanResponseDto
{
    public required string Id { get; init; }
    public required int ThreatScore { get; init; }
    public required string Verdict { get; init; }
    public required string GrokAnalysisSummary { get; init; }
    public required IReadOnlyList<string> Flags { get; init; }
    public required string RecommendedAction { get; init; }
    public string? AnalysisEngine { get; init; }
    public string? InputType { get; init; }
}
