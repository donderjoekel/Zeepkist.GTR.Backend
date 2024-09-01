namespace TNRD.Zeepkist.GTR.Backend.Stats.Resources;

public class StatsResource
{
    public string Level { get; set; } = null!;
    public Dictionary<string, decimal> Data { get; set; } = new();
}
