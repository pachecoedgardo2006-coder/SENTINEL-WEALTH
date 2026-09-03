using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Application.Services;

public sealed class FraudScannerService : IFraudScannerService
{
    private static int _scanSequence = 5041;

    private readonly IGrokApiClient _grokApiClient;

    public FraudScannerService(IGrokApiClient grokApiClient)
    {
        _grokApiClient = grokApiClient;
    }

    public async Task<ScanResponseDto> AnalyzeAsync(ScanRequestDto request, CancellationToken cancellationToken)
    {
        var analysis = await _grokApiClient.AnalyzeAsync(
            request.InputType,
            request.Content,
            cancellationToken);

        var scanId = $"SCN-{Interlocked.Increment(ref _scanSequence)}";

        return new ScanResponseDto
        {
            Id = scanId,
            ThreatScore = analysis.ThreatScore,
            Verdict = analysis.Verdict,
            GrokAnalysisSummary = analysis.GrokAnalysisSummary,
            Flags = analysis.Flags,
            RecommendedAction = analysis.RecommendedAction,
            AnalysisEngine = analysis.Engine,
            InputType = request.InputType
        };
    }
}
