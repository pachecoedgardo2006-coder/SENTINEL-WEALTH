namespace SentinelWealth.Api.Domain.Entities;

public sealed class Asset
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Class { get; init; }
    public required decimal Value { get; init; }
    public required decimal AllocationPercent { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public string? Ticker { get; init; }
    public decimal? DayChangePercent { get; init; }
    public string? Custodian { get; init; }
}
