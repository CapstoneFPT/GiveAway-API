using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dao;

public class GenericDao<T> where T : class
{
   private readonly GiveAwayDbContext _context;

   public GenericDao()
   {
      _context = new GiveAwayDbContext();
   }

   public IQueryable<T> GetQueryable()
   {
      return _context.Set<T>().AsQueryable();
   }
   
   public  void AddAsync(T entity)
   {
      _context.Set<T>().Add(entity);
   }
   
   public  void UpdateAsync(T entity)
   {
      _context.Set<T>().Update(entity);
   }
   
   public void  DeleteAsync(T entity)
   {
      _context.Set<T>().Remove(entity);
   }
   
   public Task SaveChangesAsync()
   {
      return _context.SaveChangesAsync();
   }
  
}