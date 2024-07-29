using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;

namespace Repositories.Timeslots
{
    public interface ITimeslotRepository
    {
        Task<(List<T> Items, int Page, int PageSize, int Total)> GetTimeslotProjections<T>(int? requestPageNumber,
            int? requestPageSize, Expression<Func<Timeslot, bool>> predicate, Expression<Func<Timeslot, T>> selector);
    }
}
