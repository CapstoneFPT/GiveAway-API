using BusinessObjects.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Dao;

public class GenericDao<T> where T : class
{
    private readonly GiveAwayDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;

    public GenericDao()
    {
        _dbContext = new GiveAwayDbContext();
    }

    public GenericDao(GiveAwayDbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbContext.Set<T>().AsQueryable();
    }

    public DbSet<T> GetDbSet()
    {
        return _dbContext.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new DbCustomException(e.Message,e.InnerException?.Message);
        }

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _dbContext.Set<T>().Update(entity);
            
            await _dbContext.SaveChangesAsync();
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
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
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
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new DbCustomException(instance: e.Message,e.InnerException?.Message);
        }
    }

}