using Microsoft.AspNetCore.Mvc;
using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
public sealed class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>Resumen de la cartera HNW protegida ($12.5M).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(PortfolioSummaryDto), StatusCodes.Status200OK)]
    public ActionResult<PortfolioSummaryDto> GetSummary()
        => Ok(_portfolioService.GetSummary());
}
