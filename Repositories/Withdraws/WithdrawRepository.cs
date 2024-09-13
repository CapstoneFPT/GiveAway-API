using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Withdraws;

public class WithdrawRepository : IWithdrawRepository
{
    private readonly GiveAwayDbContext _giveAwayDbContext;
    private const string Prefix = "WDR";
    private static Random _random = new();

    public WithdrawRepository(GiveAwayDbContext giveAwayDbContext)
    {
        _giveAwayDbContext = giveAwayDbContext;
    }

    public async Task<Withdraw> CreateWithdraw(Withdraw withdraw)
    {
        withdraw.WithdrawCode = await GenerateUniqueString();
        return await GenericDao<Withdraw>.Instance.AddAsync(withdraw);
    }

    public async Task<string> GenerateUniqueString()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            string code = GenerateCode();
            bool isCodeExisted = await _giveAwayDbContext.Recharges.AnyAsync(r => r.RechargeCode == code);

            if (!isCodeExisted)
            {
                return code;
            }

            await Task.Delay(100 * (int)Math.Pow(2, attempt));
        }

        throw new Exception("Failed to generate unique code after multiple attempts");
    }

    private static string GenerateCode()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string randomString = _random.Next(1000, 9999).ToString();
        return $"{Prefix}-{timestamp}-{randomString}";
    }


    public async Task<Withdraw?> GetSingleWithdraw(Expression<Func<Withdraw, bool>> predicate)
    {
        var result = await GenericDao<Withdraw>.Instance.GetQueryable()
            .FirstOrDefaultAsync(predicate);

        return result;
    }

    public async Task<Withdraw> UpdateWithdraw(Withdraw withdraw)
    {
       return await GenericDao<Withdraw>.Instance.UpdateAsync(withdraw);
    }

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetWithdraws<T>(int? requestPage,
        int? requestPageSize, Expression<Func<Withdraw, bool>> predicate, Expression<Func<Withdraw, T>> selector,
        bool isTracking, Expression<Func<Withdraw, DateTime>> orderBy)
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
            result = await query.OrderByDescending(orderBy).Select(selector).ToListAsync();
        }
        else
        {
            result = await query.OrderByDescending(orderBy).Cast<T>().ToListAsync();
        }

        return (result, page, pageSize, total);
    }
}