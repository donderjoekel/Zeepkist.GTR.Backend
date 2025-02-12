using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database.Data;

namespace TNRD.Zeepkist.GTR.Backend.DataStore;

public interface IDatabase
{
    DbSet<TModel> GetDbSet<TModel>()
        where TModel : class;

    int SaveChanges();
}

public class Database : IDatabase
{
    private readonly GtrExperimentContext _db;

    public Database(GtrExperimentContext db)
    {
        _db = db;
        _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<TModel> GetDbSet<TModel>()
        where TModel : class
    {
        return _db.Set<TModel>();
    }

    public int SaveChanges()
    {
        return _db.SaveChanges();
    }
}
