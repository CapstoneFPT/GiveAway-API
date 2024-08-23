using BusinessObjects.Entities;
using Dao;

namespace Repositories.BankAccounts;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly GiveAwayDbContext _context;

    public BankAccountRepository(GiveAwayDbContext context)
    {
        _context = context;
    }

    public IQueryable<BankAccount> GetQueryable()
    {
        return _context.BankAccounts.AsQueryable();
    }

    public async Task<BankAccount> CreateBankAccount(BankAccount bankAccount)
    {
        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();

        return bankAccount;
    }

    public async Task<BankAccount> UpdateBankAccount(BankAccount existedBankAccount)
    {
        _context.BankAccounts.Update(existedBankAccount);
         await _context.SaveChangesAsync();

        return existedBankAccount;
    }

    public Task UpdateRange(List<BankAccount> otherBankAccounts)
    {
        _context.BankAccounts.UpdateRange(otherBankAccounts);
        return _context.SaveChangesAsync();
    }

    public Task DeleteBankAccount(BankAccount existedBankAccount)
    {
        _context.BankAccounts.Remove(existedBankAccount);
        return _context.SaveChangesAsync();
    }
}

public interface IBankAccountRepository
{
    public IQueryable<BankAccount> GetQueryable();
    Task<BankAccount> CreateBankAccount(BankAccount bankAccount);
    Task<BankAccount> UpdateBankAccount(BankAccount existedBankAccount);
    Task UpdateRange(List<BankAccount> otherBankAccounts);
    Task DeleteBankAccount(BankAccount existedBankAccount);
}