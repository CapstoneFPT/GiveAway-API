using System.Linq.Expressions;
using BusinessObjects.Entities;
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
        try
        {
            var result = await _pointPackageDao.GetQueryable().FirstOrDefaultAsync(predicate);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task AddPointsToBalance(Guid accountId, int amount)
    {
        try
        {
            var account = await _accountDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.AccountId == accountId);
            
            account.Balance += amount;
            await _accountDao.UpdateAsync(account);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }
    
}