namespace TNRD.Zeepkist.GTR.Backend.Database.Models;

public partial class Favorite : IModel
{
    public int Id { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateUpdated { get; set; }

    public int? Level { get; set; }

    public int? User { get; set; }

    public virtual Level? LevelNavigation { get; set; }

    public virtual User? UserNavigation { get; set; }
}
