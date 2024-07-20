using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Withdraws;

public class WithdrawRepository : IWithdrawRepository
{
    public async Task<Withdraw> CreateWithdraw(Withdraw withdraw)
    {
        return await GenericDao<Withdraw>.Instance.AddAsync(withdraw);
    }

    public async Task<Withdraw?> GetSingleWithdraw(Expression<Func<Withdraw, bool>> predicate)
    {
        var result = await GenericDao<Withdraw>.Instance.GetQueryable()
            .FirstOrDefaultAsync(predicate);

        return result;
    }

    public async Task UpdateWithdraw(Withdraw withdraw)
    {
        await GenericDao<Withdraw>.Instance.UpdateAsync(withdraw);
    }
}