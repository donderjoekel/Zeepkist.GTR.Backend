using System.ComponentModel.DataAnnotations;

namespace TNRD.Zeepkist.GTR.Backend.Database.Models;

internal class User
{
    [Key] public int Id { get; set; }

    [Required] public DateTime DateCreated { get; set; }

    [Required] public DateTime DateUpdated { get; set; }

    public string RefreshToken { get; set; }

    public string RefreshTokenExpiry { get; set; }

    public string AccessToken { get; set; }

    public string AccessTokenExpiry { get; set; }

    public string SteamId { get; set; }

    public string SteamName { get; set; }

    public virtual ICollection<Record> Records { get; set; }
}
