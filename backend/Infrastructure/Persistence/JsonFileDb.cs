using System.Text.Json;
using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Domain.Entities;

namespace SentinelWealth.Api.Infrastructure.Persistence;

public sealed class JsonFileDb : IPortfolioRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<StoredUser> _users;

    public JsonFileDb(IWebHostEnvironment env, ILogger<JsonFileDb> logger)
    {
        var path = ResolveDbPath(env);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"No se encontró la base JSON en {path}");
        }

        var json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<DbRoot>(json, JsonOptions)
                   ?? throw new InvalidOperationException("db.json inválido.");
        _users = root.Users ?? [];
        logger.LogInformation("JSON DB cargada: {Count} usuarios desde {Path}", _users.Count, path);
    }

    public UserAccount? FindUser(string username, string password)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
            && u.Password == password);

        if (user is null)
        {
            return null;
        }

        return new UserAccount
        {
            Id = user.Id,
            Username = user.Username,
            ClientName = user.ClientName,
            Tier = user.Tier
        };
    }

    public Portfolio? GetByUserId(string userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId || string.Equals(u.Username, userId, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        return new Portfolio
        {
            Id = user.Id,
            ClientName = user.ClientName,
            Tier = user.Tier,
            Domicile = user.Domicile,
            TotalNetWorth = user.TotalNetWorth,
            Currency = user.Currency,
            RiskScore = user.RiskScore,
            RiskNumeric = user.RiskNumeric,
            Assets = (user.Assets ?? []).Select(a => new Asset
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
            RecentAlerts = (user.RecentAlerts ?? []).Select(alert => new Alert
            {
                Id = alert.Id,
                Type = alert.Type,
                Severity = alert.Severity,
                Title = alert.Title,
                Description = alert.Description,
                Status = alert.Status,
                DetectedAt = now.AddMinutes(-(alert.DetectedMinutesAgo <= 0 ? 5 : alert.DetectedMinutesAgo)),
                AnalyzedBy = alert.AnalyzedBy,
                AmountUsd = alert.AmountUsd,
                SourceIp = alert.SourceIp,
                RecommendedAction = alert.RecommendedAction
            }).ToList()
        };
    }

    private static string ResolveDbPath(IWebHostEnvironment env)
    {
        var candidates = new[]
        {
            Path.Combine(env.ContentRootPath, "Data", "db.json"),
            Path.Combine(AppContext.BaseDirectory, "Data", "db.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "db.json")
        };

        return candidates.FirstOrDefault(File.Exists)
               ?? Path.Combine(env.ContentRootPath, "Data", "db.json");
    }

    private sealed class DbRoot
    {
        public List<StoredUser>? Users { get; set; }
    }

    private sealed class StoredUser
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Tier { get; set; } = "";
        public string Domicile { get; set; } = "";
        public decimal TotalNetWorth { get; set; }
        public string Currency { get; set; } = "USD";
        public string RiskScore { get; set; } = "";
        public int RiskNumeric { get; set; }
        public List<StoredAsset>? Assets { get; set; }
        public List<StoredAlert>? RecentAlerts { get; set; }
    }

    private sealed class StoredAsset
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Class { get; set; } = "";
        public decimal Value { get; set; }
        public decimal AllocationPercent { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "";
        public string? Ticker { get; set; }
        public decimal? DayChangePercent { get; set; }
        public string? Custodian { get; set; }
    }

    private sealed class StoredAlert
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public int DetectedMinutesAgo { get; set; }
        public string? AnalyzedBy { get; set; }
        public decimal? AmountUsd { get; set; }
        public string? SourceIp { get; set; }
        public string? RecommendedAction { get; set; }
    }
}
