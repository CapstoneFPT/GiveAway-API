using BusinessObjects.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Dao;

public class GenericDao<T> where T : class
{
    private static readonly object _lock = new object();
    private static GenericDao<T>? _instance = null;

    private GenericDao()
    {
    }

    public static GenericDao<T> Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new GenericDao<T>();
                }

                return _instance;
            }
        }
    }

    // public GenericDao(GiveAwayDbContext dbContext)
    // {
    //     _dbContext = dbContext;
    // }

    public IQueryable<T> GetQueryable()
    {
        var dbContext = new GiveAwayDbContext();
        return dbContext.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            var dbContext = new GiveAwayDbContext();

            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync();

            return entity;
        }
        catch (Exception e)
        {
            throw new DbCustomException(e.Message, e.InnerException?.Message);
        }
    }
    public async Task<List<T>> AddRange(List<T> entity)
    {
        try
        {
            var dbContext = new GiveAwayDbContext();

            await dbContext.Set<T>().AddRangeAsync(entity);
            await dbContext.SaveChangesAsync();

            return entity;
        }
        catch (Exception e)
        {
            throw new DbCustomException(e.Message, e.InnerException?.Message);
        }
    }
    public async Task<T> UpdateAsync(T entity)
    {
        try
        {
            var dbContext = new GiveAwayDbContext();

            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new DbCustomException(instance: e.Message, e.InnerException?.Message);
        }
    }

    public async Task<List<T>> UpdateRange(List<T> entities)
    {
        try
        {
            var dbContext = new GiveAwayDbContext();
            dbContext.Set<T>().UpdateRange(entities);
            await dbContext.SaveChangesAsync();
            return entities;
        }
        catch (Exception e)
        {
            throw new DbCustomException(instance: e.Message, e.InnerException?.Message);
        }
    }


    public async Task<T> DeleteAsync(T entity)
    {
        try
        {
            var dbContext = new GiveAwayDbContext();
            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new DbCustomException(instance: e.Message, e.InnerException?.Message);
        }
    }
}