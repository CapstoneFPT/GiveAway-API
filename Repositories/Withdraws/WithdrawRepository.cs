using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Withdraws;

public class WithdrawRepository : IWithdrawRepository
{
    private readonly GiveAwayDbContext _giveAwayDbContext;

    public WithdrawRepository(GiveAwayDbContext giveAwayDbContext)
    {
        _giveAwayDbContext = giveAwayDbContext;
    }

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

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetWithdraws<T>(int? requestPage,
        int? requestPageSize, Expression<Func<Withdraw, bool>> predicate, Expression<Func<Withdraw, T>> selector,
        bool isTracking)
    {
        var query = _giveAwayDbContext.Withdraws.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        if (!isTracking)
        {
            query = query.AsNoTracking();
        }

        var total = await query.CountAsync();

        var page = requestPage ?? -1;
        var pageSize = requestPageSize ?? -1;

        if (page > 0 && pageSize >= 0)
        {
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        List<T> result;
        
        if (selector != null)
        {
            result = await query.Select(selector).ToListAsync();
        }
        else
        {
            result = await query.Cast<T>().ToListAsync();
        }

        return (result, page, pageSize, total);
    }
}