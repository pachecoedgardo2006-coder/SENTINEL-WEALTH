using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.Services;
using SentinelWealth.Api.Infrastructure.Grok;
using SentinelWealth.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<InMemoryMockDb>();
builder.Services.AddSingleton<IPortfolioRepository>(sp => sp.GetRequiredService<InMemoryMockDb>());
builder.Services.AddSingleton<HeuristicFraudAnalyzer>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IFraudScannerService, FraudScannerService>();

builder.Services.AddHttpClient<IGrokApiClient, GrokApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SentinelWealth-AI/1.0");
});

var app = builder.Build();

app.MapOpenApi();
app.UseCors("OpenCors");
app.MapGet("/", () => Results.Ok(new
{
    service = "SentinelWealth.Api",
    docs = "/openapi/v1.json",
    endpoints = new[]
    {
        "GET /api/health",
        "GET /api/portfolio/summary",
        "POST /api/scanner/analyze"
    }
}));
app.MapControllers();

app.Run();
