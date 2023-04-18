using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNRD.Zeepkist.GTR.Backend.Database.Models;

internal class Level
{
    [Key] public int Id { get; set; }

    [Required] public DateTime DateCreated { get; set; }

    [Required] public DateTime DateUpdated { get; set; }

    public string Uid { get; set; }

    public string Name { get; set; }

    public string Author { get; set; }

    public float TimeAuthor { get; set; }

    [Required] public float TimeGold { get; set; }

    public float? TimeSilver { get; set; }

    public float? TimeBronze { get; set; }

    [Required] [ForeignKey("User")] public int CreatedBy { get; set; }

    public string Wid { get; set; }

    [Required] public bool IsValid { get; set; }

    public string ThumbnailUrl { get; set; }

    [ForeignKey("CreatedBy")] public virtual User User { get; set; }
}
