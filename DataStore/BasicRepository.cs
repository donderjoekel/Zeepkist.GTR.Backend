using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNRD.Zeepkist.GTR.Database;

namespace TNRD.Zeepkist.GTR.Backend.DataStore;

public interface IBasicRepository<TModel>
    where TModel : class, IEntity
{
    int Count();
    int Count(Expression<Func<TModel, bool>> predicate);
    bool Exists(Expression<Func<TModel, bool>> predicate);
    TModel? GetById(int id);
    TModel? GetById(int id, Func<DbSet<TModel>, IQueryable<TModel>> set);
    IEnumerable<TModel> GetAll();
    IEnumerable<TModel> GetAll(Func<DbSet<TModel>, IQueryable<TModel>> set);
    IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate);
    IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate, Func<DbSet<TModel>, IQueryable<TModel>> set);
    TModel? GetSingle(Expression<Func<TModel, bool>> predicate);
    TModel? GetSingle(Expression<Func<TModel, bool>> predicate, Func<DbSet<TModel>, IQueryable<TModel>> set);
    TModel Insert(TModel model);
    TModel Insert(TModel model, DateTimeOffset dateCreated);
    void InsertRange(IEnumerable<TModel> models);
    TModel Update(TModel model);
    void UpdateRange(IEnumerable<TModel> models);

    TModel Upsert(
        int id,
        Func<TModel> insert,
        Func<TModel, TModel> update);

    TModel Upsert(
        Expression<Func<TModel, bool>> predicate,
        Func<TModel> insert,
        Func<TModel, TModel> update);

    bool Delete(TModel model);
}

public abstract class BasicRepository<TModel> : IBasicRepository<TModel>
    where TModel : class, IEntity
{
    private readonly IDatabase _database;
    private readonly DbSet<TModel> _set;
    private readonly ILogger _logger;

    protected BasicRepository(IDatabase database, ILogger logger)
    {
        _database = database;
        _logger = logger;
        _set = _database.GetDbSet<TModel>();
    }

    public int Count()
    {
        return _set.Count();
    }

    public int Count(Expression<Func<TModel, bool>> predicate)
    {
        return _set.Count(predicate);
    }

    public bool Exists(Expression<Func<TModel, bool>> predicate)
    {
        return _set.Any(predicate);
    }

    public TModel? GetById(int id)
    {
        return _set.SingleOrDefault(x => x.Id == id);
    }

    public TModel? GetById(int id, Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return set.Invoke(_set).SingleOrDefault(x => x.Id == id);
    }

    public IEnumerable<TModel> GetAll()
    {
        return _set;
    }

    public IEnumerable<TModel> GetAll(Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return set.Invoke(_set);
    }

    public IEnumerable<TModel> GetAll(
        Expression<Func<TModel, bool>> predicate,
        Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return set.Invoke(_set).Where(predicate);
    }

    public IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate)
    {
        return _set.Where(predicate);
    }

    public TModel? GetSingle(Expression<Func<TModel, bool>> predicate)
    {
        return _set.FirstOrDefault(predicate);
    }

    public TModel? GetSingle(Expression<Func<TModel, bool>> predicate, Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return set.Invoke(_set).FirstOrDefault(predicate);
    }

    public TModel Insert(TModel model)
    {
        return Insert(model, DateTimeOffset.UtcNow);
    }

    public TModel Insert(TModel model, DateTimeOffset dateCreated)
    {
        model.DateCreated = dateCreated;
        EntityEntry<TModel> entry = _set.Entry(model);
        entry.State = EntityState.Added;
        _database.SaveChanges();
        entry.State = EntityState.Detached;
        return entry.Entity;
    }

    public void InsertRange(IEnumerable<TModel> models)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (TModel entity in models)
        {
            entity.DateCreated = now;
        }

        _set.AddRange(models);
        _database.SaveChanges();
    }

    public TModel Update(TModel model)
    {
        model.DateUpdated = DateTimeOffset.UtcNow;
        EntityEntry<TModel> entry = _set.Entry(model);
        entry.State = EntityState.Modified;
        _database.SaveChanges();
        entry.State = EntityState.Detached;
        return entry.Entity;
    }

    public void UpdateRange(IEnumerable<TModel> models)
    {
        foreach (TModel model in models)
        {
            model.DateUpdated = DateTimeOffset.UtcNow;
        }

        _set.UpdateRange(models);
        _database.SaveChanges();
    }

    public TModel Upsert(
        int id,
        Func<TModel> insert,
        Func<TModel, TModel> update)
    {
        TModel? model = GetById(id);
        return model == null
            ? Insert(insert.Invoke())
            : Update(update(model));
    }

    public TModel Upsert(
        Expression<Func<TModel, bool>> predicate,
        Func<TModel> insert,
        Func<TModel, TModel> update)
    {
        TModel? model = GetSingle(predicate);
        return model == null
            ? Insert(insert.Invoke())
            : Update(update(model));
    }

    public bool Delete(TModel model)
    {
        _set.Remove(model);
        int changes = _database.SaveChanges();
        return changes > 0;
    }
}
