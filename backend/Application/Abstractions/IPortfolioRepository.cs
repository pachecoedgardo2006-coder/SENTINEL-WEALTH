using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Application.Abstractions;

public interface IPortfolioRepository
{
    Portfolio? GetByUserId(string userId);
    UserAccount? FindUser(string username, string password);
}

public sealed class UserAccount
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string ClientName { get; init; }
    public required string Tier { get; init; }
}
