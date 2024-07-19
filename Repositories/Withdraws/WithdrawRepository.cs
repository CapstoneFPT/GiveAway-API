using BusinessObjects.Entities;
using Dao;

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
}