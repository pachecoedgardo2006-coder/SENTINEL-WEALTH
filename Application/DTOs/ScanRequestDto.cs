using System.ComponentModel.DataAnnotations;

namespace SentinelWealth.Api.Application.DTOs;

public sealed class ScanRequestDto
{
    /// <summary>text | url | contract</summary>
    [Required]
    public string InputType { get; set; } = "text";

    [Required]
    public string Content { get; set; } = string.Empty;
}
