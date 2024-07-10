using System.Linq.Expressions;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PointPackages;

public class PointPackageRepository : IPointPackageRepository
{
    private readonly GenericDao<PointPackage> _pointPackageDao;
    private readonly GenericDao<Account> _accountDao;

    public PointPackageRepository(GenericDao<PointPackage> pointPackageDao, GenericDao<Account> accountDao)
    {
        _pointPackageDao = pointPackageDao;
        _accountDao = accountDao;
    }

    public async Task<PointPackage?> GetSingle(Expression<Func<PointPackage, bool>> predicate)
    {
        var result = await _pointPackageDao.GetQueryable().FirstOrDefaultAsync(predicate);
        return result;
    }

    public async Task AddPointsToBalance(Guid accountId, int amount)
    {
        var account = await _accountDao.GetQueryable()
            .FirstOrDefaultAsync(x => x.AccountId == accountId);

        if (account == null)
        {
            throw new AccountNotFoundException();
        }

        account.Balance += amount;
        await _accountDao.UpdateAsync(account);
    }
}