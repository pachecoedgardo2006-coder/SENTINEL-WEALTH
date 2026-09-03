using Microsoft.AspNetCore.Mvc;

namespace SentinelWealth.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "SentinelWealth AI",
        runtime = "NET 10",
        utc = DateTimeOffset.UtcNow
    });
}
