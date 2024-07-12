using System.Linq.Expressions;
using BusinessObjects.Dtos.PointPackages;
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

    public async Task<T?> GetSingle<T>(Expression<Func<PointPackage, bool>> predicate, Expression<Func<PointPackage, T>>? selector)
    {
        var query = _pointPackageDao.GetQueryable();

        if (selector != null)
        {
            return await query
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();
        }
        
        return await query
            .Where(predicate)
            .Cast<T>()
            .FirstOrDefaultAsync();
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

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetPointPackages<T>(
        int page, int pageSize,
        Expression<Func<PointPackage, bool>> predicate,
        Expression<Func<PointPackage, T>> selector)
    {
        var query = _pointPackageDao.GetQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        var total = query.Count();
       
        if(page != 0 && pageSize != 0)
        {
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        List<T> items;
        if (selector != null)
        {
            items = await query.Select(selector).ToListAsync();
        }
        else
        {
            items = await query.Cast<T>().ToListAsync();
        }
        
        return (items, page, pageSize, total);
    }
}