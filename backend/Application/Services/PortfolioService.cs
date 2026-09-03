using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Application.Services;

public sealed class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repository;

    public PortfolioService(IPortfolioRepository repository)
    {
        _repository = repository;
    }

    public PortfolioSummaryDto? GetSummary(string userId)
    {
        var portfolio = _repository.GetByUserId(userId);
        if (portfolio is null)
        {
            return null;
        }

        return new PortfolioSummaryDto
        {
            TotalNetWorth = portfolio.TotalNetWorth,
            Currency = portfolio.Currency,
            RiskScore = portfolio.RiskScore,
            RiskNumeric = portfolio.RiskNumeric,
            ClientName = portfolio.ClientName,
            Tier = portfolio.Tier,
            Domicile = portfolio.Domicile,
            ShieldStatus = "Activo — blindaje HNW SentinelWealth",
            Assets = portfolio.Assets.Select(a => new AssetDto
            {
                Id = a.Id,
                Name = a.Name,
                Class = a.Class,
                Value = a.Value,
                AllocationPercent = a.AllocationPercent,
                Currency = a.Currency,
                Status = a.Status,
                Ticker = a.Ticker,
                DayChangePercent = a.DayChangePercent,
                Custodian = a.Custodian
            }).ToList(),
            RecentAlerts = portfolio.RecentAlerts.Select(alert => new AlertDto
            {
                Id = alert.Id,
                Type = alert.Type,
                Severity = alert.Severity,
                Title = alert.Title,
                Description = alert.Description,
                Status = alert.Status,
                DetectedAt = alert.DetectedAt,
                AnalyzedBy = alert.AnalyzedBy,
                AmountUsd = alert.AmountUsd,
                SourceIp = alert.SourceIp,
                RecommendedAction = alert.RecommendedAction
            }).ToList()
        };
    }
}
