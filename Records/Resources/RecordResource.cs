using System.ComponentModel.DataAnnotations;

namespace TNRD.Zeepkist.GTR.Backend.Records.Resources;

public class RecordResource
{
    [Required] public string Level { get; set; } = null!;
    [Required, Range(0, float.MaxValue)] public float Time { get; set; }
    [Required] public List<float> Splits { get; set; } = null!;
    [Required] public List<float> Speeds { get; set; } = null!;
    [Required] public string GhostData { get; set; } = null!;
    [Required] public string GameVersion { get; set; } = null!;
    [Required] public string ModVersion { get; set; } = null!;
}
