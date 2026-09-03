using System.Text.Json.Serialization;

namespace SentinelWealth.Api.Application.DTOs;

public sealed class ScanRequestDto
{
    /// <summary>text | url | contract</summary>
    public string InputType { get; set; } = "text";

    public string Content { get; set; } = string.Empty;

    /// <summary>Alias que envía el frontend (campo text).</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public string ResolvedContent() =>
        !string.IsNullOrWhiteSpace(Content) ? Content : Text ?? string.Empty;
}
