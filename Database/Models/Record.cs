using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNRD.Zeepkist.GTR.Backend.Database.Models;

internal class Record
{
    [Key] public int Id { get; set; }

    [Required] public DateTime DateCreated { get; set; }

    [Required] public DateTime DateUpdated { get; set; }

    [ForeignKey("Level")] public int LevelId { get; set; }

    public virtual Level Level { get; set; }

    [ForeignKey("User")] public int? UserId { get; set; }

    public virtual User User { get; set; }

    public float Time { get; set; }

    [Required] public bool IsBest { get; set; }

    public string Splits { get; set; }

    public string GhostUrl { get; set; }

    public string ScreenshotUrl { get; set; }

    public string GameVersion { get; set; }

    [Required] public bool IsValid { get; set; }

    [Required] public bool IsWr { get; set; }
}
