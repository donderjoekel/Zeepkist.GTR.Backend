namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Add;

internal class LevelsAddRequestDTO
{
    public string Uid { get; set; } = null!;
    public string Wid { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public bool IsValid { get; set; }

    public float TimeAuthor { get; set; }
    public float TimeGold { get; set; }
    public float TimeSilver { get; set; }
    public float TimeBronze { get; set; }
    
    public string Thumbnail { get; set; } = null!;
}
