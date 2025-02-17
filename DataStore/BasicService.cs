using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;

namespace TNRD.Zeepkist.GTR.Backend.DataStore;

public interface IBasicService<TModel>
    where TModel : class, IEntity
{
    IEnumerable<TModel> GetAll();
    IEnumerable<TModel> GetAll(Func<DbSet<TModel>, IQueryable<TModel>> set);
    IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate);
    IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate, Func<DbSet<TModel>, IQueryable<TModel>> set);
    TModel? GetById(int id);
    TModel Insert(TModel model);
    TModel Update(TModel model);
    bool Delete(TModel model);
    bool Delete(IEnumerable<TModel> models);
}

public class BasicService<TModel> : IBasicService<TModel>
    where TModel : class, IEntity
{
    private readonly IBasicRepository<TModel> _repository;

    public BasicService(IBasicRepository<TModel> repository)
    {
        _repository = repository;
    }

    public IEnumerable<TModel> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<TModel> GetAll(Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return _repository.GetAll(set);
    }

    public IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate)
    {
        return _repository.GetAll(predicate);
    }

    public IEnumerable<TModel> GetAll(Expression<Func<TModel, bool>> predicate,
        Func<DbSet<TModel>, IQueryable<TModel>> set)
    {
        return _repository.GetAll(predicate, set);
    }

    public TModel? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public TModel Insert(TModel model)
    {
        return _repository.Insert(model);
    }

    public TModel Update(TModel model)
    {
        return _repository.Update(model);
    }

    public bool Delete(TModel model)
    {
        return _repository.Delete(model);
    }

    public bool Delete(IEnumerable<TModel> models)
    {
        return _repository.Delete(models);
    }
}
