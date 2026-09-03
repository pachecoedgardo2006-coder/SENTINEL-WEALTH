namespace SentinelWealth.Api.Domain.Entities;

public sealed class Portfolio
{
    public required string Id { get; init; }
    public required string ClientName { get; init; }
    public required string Tier { get; init; }
    public required string Domicile { get; init; }
    public required decimal TotalNetWorth { get; init; }
    public required string Currency { get; init; }
    public required string RiskScore { get; init; }
    public required int RiskNumeric { get; init; }
    public required IReadOnlyList<Asset> Assets { get; init; }
    public required IReadOnlyList<Alert> RecentAlerts { get; init; }
}
