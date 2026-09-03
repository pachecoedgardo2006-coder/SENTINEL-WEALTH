using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Application.Abstractions;

public interface IGrokApiClient
{
    Task<ScanAnalysis> AnalyzeAsync(string inputType, string content, CancellationToken cancellationToken);
}
