namespace LuminaryLife.Common.Systems.Tags.Models;

/// <summary>
/// Response DTO for tag information
/// </summary>
public class TagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HexColorCode { get; set; }
}
