using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Timeslots
{
    public class TimeslotRepository : ITimeslotRepository
    {

        public async Task<(List<T> Items, int Page, int PageSize, int Total)> GetTimeslotProjections<T>(
            int? requestPageNumber, int? requestPageSize, Expression<Func<Timeslot, bool>> predicate,
            Expression<Func<Timeslot, T>> selector)
        {
            var query = GenericDao<Timeslot>.Instance.GetQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();
            
            var page = requestPageNumber ?? -1;
            var pageSize = requestPageSize ?? -1;
            
            if(page >= 0 && pageSize >= 0)
            {
                query = query.Skip((page-1) * pageSize).Take(pageSize);
            }
            
            List<T> result;
            if (selector != null)
            {
                result = await query.Select(selector).ToListAsync();
            }
            else
            {
                result = await query.Cast<T>().ToListAsync();
            }
            return (result, page, pageSize, totalCount);
        }
    }
}