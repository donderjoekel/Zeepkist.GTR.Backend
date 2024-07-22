using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public abstract class StatsRepository<TModel> : BasicRepository<TModel>
    where TModel : class, IEntity
{
    protected StatsRepository(IDatabase database, ILogger logger)
        : base(database, logger)
    {
    }

    protected static string UpdateData(string current, Dictionary<string, decimal> updateData)
    {
        Dictionary<string, decimal>? currentData
            = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(current);

        if (currentData == null)
        {
            return JsonConvert.SerializeObject(updateData);
        }

        foreach (KeyValuePair<string, decimal> kvp in updateData)
        {
            if (currentData.TryGetValue(kvp.Key, out decimal existingValue))
            {
                currentData[kvp.Key] = existingValue + kvp.Value;
            }
            else
            {
                currentData[kvp.Key] = kvp.Value;
            }
        }

        return JsonConvert.SerializeObject(currentData);
    }
}
