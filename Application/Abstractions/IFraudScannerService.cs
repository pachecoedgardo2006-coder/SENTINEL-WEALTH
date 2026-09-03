using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Application.Abstractions;

public interface IFraudScannerService
{
    Task<ScanResponseDto> AnalyzeAsync(ScanRequestDto request, CancellationToken cancellationToken);
}
