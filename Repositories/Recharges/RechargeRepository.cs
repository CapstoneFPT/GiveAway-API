using System.Linq.Expressions;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Recharges;

public class RechargeRepository : IRechargeRepository
{
    private readonly GiveAwayDbContext _giveAwayDbContext;
    private const string prefix = "RCH";
    private static HashSet<string> generatedStrings = new HashSet<string>();
    private static Random random = new Random();

    public RechargeRepository(GiveAwayDbContext giveAwayDbContext)
    {
        _giveAwayDbContext = giveAwayDbContext;
    }

    public async Task<Recharge?> CreateRecharge(Recharge recharge)
    {
        recharge.RechargeCode = await GenerateUniqueString();
        await _giveAwayDbContext.Recharges.AddAsync(recharge);
        await _giveAwayDbContext.SaveChangesAsync();
        return recharge;
    }

    public async Task<Recharge?> GetRechargeById(Guid rechargeId)
    {
        return await _giveAwayDbContext.Recharges
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.RechargeId == rechargeId);
    }

    public async Task UpdateRecharge(Recharge recharge)
    {
        _giveAwayDbContext.Recharges.Update(recharge);
        await _giveAwayDbContext.SaveChangesAsync();
    }

    public IQueryable<Recharge> GetQueryable()
    {
        return _giveAwayDbContext.Recharges.AsQueryable();
    }

    public async Task<T?> GetSingle<T>(Expression<Func<Recharge, bool>> predicate,
        Expression<Func<Recharge, T>>? selector)
    {
        var query = _giveAwayDbContext.Recharges.AsQueryable();

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
        var account = await _giveAwayDbContext.Accounts
            .FirstOrDefaultAsync(x => x.AccountId == accountId);

        if (account == null)
        {
            throw new AccountNotFoundException();
        }

        account.Balance += amount;
        _giveAwayDbContext.Accounts.Update(account);
        await _giveAwayDbContext.SaveChangesAsync();
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
        string randomString = random.Next(1000, 9999).ToString();
        return $"{prefix}-{timestamp}-{randomString}";
    }

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetPointPackages<T>(
        int page, int pageSize,
        Expression<Func<Recharge, bool>> predicate,
        Expression<Func<Recharge, T>> selector)
    {
        var query = _giveAwayDbContext.Recharges.AsQueryable();
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