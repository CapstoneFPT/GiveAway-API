using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dao;

public class GenericDao<T> where T : class
{
    private readonly GiveAwayDbContext _context;

    public GenericDao()
    {
        _context = new GiveAwayDbContext();
    }

    public GenericDao(GiveAwayDbContext context)
    {
        _context = context;
    }

    public IQueryable<T> GetQueryable()
    {
        return _context.Set<T>().AsQueryable();
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<T> DeleteAsync(T entity)
    {
        try
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}