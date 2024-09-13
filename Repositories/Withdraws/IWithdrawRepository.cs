using System.Linq.Expressions;
using BusinessObjects.Entities;

namespace Repositories.Withdraws;

public interface IWithdrawRepository
{
    Task<Withdraw> CreateWithdraw(Withdraw withdraw);
    Task<Withdraw?> GetSingleWithdraw(Expression<Func<Withdraw, bool>> predicate);
    Task<Withdraw> UpdateWithdraw(Withdraw withdraw);
    Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetWithdraws<T>(int? requestPage,
        int? requestPageSize, Expression<Func<Withdraw, bool>> predicate, Expression<Func<Withdraw, T>> selector,
        bool isTracking, Expression<Func<Withdraw, DateTime>> orderBy);
}