namespace TNRD.Zeepkist.GTR.Backend.Rabbit;

public class PublishableRecord
{
    public int Id { get; set; }
    public int Level { get; set; }
    public int User { get; set; }
    public float Time { get; set; }
    public bool IsValid { get; set; }
    public bool IsBest { get; set; }
    public bool IsWorldRecord { get; set; }
}
