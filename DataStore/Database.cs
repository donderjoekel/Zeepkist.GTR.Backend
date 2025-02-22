using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TNRD.Zeepkist.GTR.Database.Data;

namespace TNRD.Zeepkist.GTR.Backend.DataStore;

public interface IDatabase
{
    Task<bool> EnsureCreated();

    DbSet<TModel> GetDbSet<TModel>()
        where TModel : class;

    int SaveChanges();
}

public class Database : IDatabase
{
    private readonly GtrExperimentContext _db;
    private readonly ILogger<Database> _logger;

    public Database(GtrExperimentContext db, ILogger<Database> logger)
    {
        _db = db;
        _logger = logger;
        _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public async Task<bool> EnsureCreated()
    {
        await Task.Delay(100);
        return true;

        // TIP: Uncomment this if you need to automatically create the db on startup

        // Assembly assembly = typeof(Database).Assembly;
        // string resourceName = assembly.GetManifestResourceNames()
        //     .Single(str => str.EndsWith("baseline.sql"));
        //
        // string sql;
        // await using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        // {
        //     using (StreamReader reader = new(stream))
        //     {
        //         sql = await reader.ReadToEndAsync();
        //     }
        // }
        //
        // IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        // try
        // {
        //     await _db.Database.ExecuteSqlRawAsync(sql);
        //     await transaction.CommitAsync();
        //     return true;
        // }
        // catch (Exception e)
        // {
        //     await transaction.RollbackAsync();
        //     _logger.LogCritical(e, "Unable to create database");
        //     return false;
        // }
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
