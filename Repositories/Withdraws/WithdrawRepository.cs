using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Withdraws;

public class WithdrawRepository : IWithdrawRepository
{
    private readonly GenericDao<Withdraw> _withdrawDao;

    public WithdrawRepository(GenericDao<Withdraw> withdrawDao)
    {
        _withdrawDao = withdrawDao;
    }

    public async Task<Withdraw> CreateWithdraw(Withdraw withdraw)
    {
        return await _withdrawDao.AddAsync(withdraw);
    }

    public async Task<Withdraw?> GetSingleWithdraw(Expression<Func<Withdraw, bool>> predicate)
    {
        var result = await _withdrawDao.GetQueryable()
            .FirstOrDefaultAsync(predicate);

        return result;
    }

    public async Task UpdateWithdraw(Withdraw withdraw)
    {
        await _withdrawDao.UpdateAsync(withdraw);
    }
}