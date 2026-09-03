using Microsoft.AspNetCore.Mvc;
using SentinelWealth.Api.Application.Abstractions;
using SentinelWealth.Api.Application.DTOs;

namespace SentinelWealth.Api.Controllers;

[ApiController]
[Route("api/scanner")]
public sealed class FraudScannerController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text", "url", "contract"
    };

    private readonly IFraudScannerService _scannerService;

    public FraudScannerController(IFraudScannerService scannerService)
    {
        _scannerService = scannerService;
    }

    /// <summary>Escáner ciudadano anti-estafas potenciado por Grok (con fallback heurístico).</summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(ScanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScanResponseDto>> Analyze(
        [FromBody] ScanRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "content es requerido." });
        }

        request.Content = request.ResolvedContent();
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { error = "content es requerido." });
        }

        if (string.IsNullOrWhiteSpace(request.InputType))
        {
            request.InputType = "text";
        }

        if (!AllowedTypes.Contains(request.InputType))
        {
            return BadRequest(new { error = "inputType debe ser text, url o contract." });
        }

        var result = await _scannerService.AnalyzeAsync(request, cancellationToken);
        return Ok(result);
    }
}
