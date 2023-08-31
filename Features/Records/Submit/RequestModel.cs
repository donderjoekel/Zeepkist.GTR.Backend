namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class RequestModel
{
    public string? LevelHash { get; set; }
    public int Level { get; set; }
    public int User { get; set; }
    public float Time { get; set; }
    public float[] Splits { get; set; } = null!;
    public string GhostData { get; set; } = null!;
    public string ScreenshotData { get; set; } = null!;
    public bool IsValid { get; set; }
    public string GameVersion { get; set; } = null!;
}
