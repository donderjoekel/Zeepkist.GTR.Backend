namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class ResponseModel
{
    public int Id { get; set; }
    public int Level { get; set; }
    public int User { get; set; }
    public float Time { get; set; }
    public float[] Splits { get; set; } = null!;
    public string GhostUrl { get; set; } = null!;
    public string ScreenshotUrl { get; set; } = null!;
    public bool IsValid { get; set; }
    public bool IsBest { get; set; }
    public bool IsWorldRecord { get; set; }
    public string GameVersion { get; set; } = null!;
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
