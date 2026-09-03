using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Application.Abstractions;

public interface IPortfolioRepository
{
    Portfolio GetProtectedPortfolio();
}
