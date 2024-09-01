namespace TNRD.Zeepkist.GTR.Backend.Workshop;

public class WorkshopDownloads
{
    public WorkshopDownloads(string id, IEnumerable<WorkshopItem> items)
    {
        Id = id;
        Items = items;
    }

    public string Id { get; private set; }
    public IEnumerable<WorkshopItem> Items { get; private set; }
}
