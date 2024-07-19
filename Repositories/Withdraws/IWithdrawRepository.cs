using System.Linq.Expressions;
using BusinessObjects.Entities;

namespace Repositories.Withdraws;

public interface IWithdrawRepository
{
    Task<Withdraw> CreateWithdraw(Withdraw withdraw);
    Task<Withdraw?> GetSingleWithdraw(Expression<Func<Withdraw, bool>> predicate);
    Task UpdateWithdraw(Withdraw withdraw);
}