using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Application.Abstractions;

public interface IPortfolioService
{
    PortfolioSummaryDto? GetSummary(string userId);
}
