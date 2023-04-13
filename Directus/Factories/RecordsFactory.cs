using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class RecordsFactory : BaseFactory<RecordsFactory>
{
    public RecordsFactory WithGhostUrl(string ghostUrl)
    {
        model.ghost_url = ghostUrl;
        return this;
    }
    
    public RecordsFactory WithScreenshotUrl(string screenshotUrl)
    {
        model.screenshot_url = screenshotUrl;
        return this;
    }
    
    public RecordsFactory WithIsBestRun(bool isBestRun)
    {
        model.is_best = isBestRun;
        return this;
    }

    public RecordsFactory WithIsWorldRecord(bool isWorldRecord)
    {
        model.is_wr = isWorldRecord;
        return this;
    }
}
