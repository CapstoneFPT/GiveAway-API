using System.Linq.Expressions;
using BusinessObjects.Dtos.PointPackages;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PointPackages;

public class PointPackageRepository : IPointPackageRepository
{
    private readonly GiveAwayDbContext _giveAwayDbContext;

    public PointPackageRepository(GiveAwayDbContext giveAwayDbContext)
    {
        _giveAwayDbContext = giveAwayDbContext;
    }

    public async Task<T?> GetSingle<T>(Expression<Func<PointPackage, bool>> predicate,
        Expression<Func<PointPackage, T>>? selector)
    {
        var query = GenericDao<PointPackage>.Instance.GetQueryable();

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
        // var account = await _giveAwayDbContext.Accounts
        //     .FirstOrDefaultAsync(x => x.AccountId == accountId);
        //
        // if (account == null)
        // {
        //     throw new AccountNotFoundException();
        // }
        // account.Balance += amount;
        // _giveAwayDbContext.Accounts.Update(account);
        // await _giveAwayDbContext.SaveChangesAsync();

        await _giveAwayDbContext.Accounts.Where(x => x.AccountId == accountId).ExecuteUpdateAsync(s =>
            s.SetProperty(account => account.Balance, account => account.Balance + amount));
    }

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetPointPackages<T>(
        int page, int pageSize,
        Expression<Func<PointPackage, bool>> predicate,
        Expression<Func<PointPackage, T>> selector)
    {
        var query = GenericDao<PointPackage>.Instance.GetQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var total = await query.CountAsync();

        if (page != 0 && pageSize != 0)
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