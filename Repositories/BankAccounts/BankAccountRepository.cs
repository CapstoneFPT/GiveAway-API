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
}

public interface IBankAccountRepository
{
    public IQueryable<BankAccount> GetQueryable();
    Task<BankAccount> CreateBankAccount(BankAccount bankAccount);
}