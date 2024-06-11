using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Accounts
{
    public interface IAccountRepository
    {
        Task<List<Account>> FindMany(
        Func<Account, bool> predicate,
        int page,
        int pageSize
    );
        Task<Account?> FindOne(Func<Account, bool> predicate);
        Task ResetPassword(Guid uid, string password);
        Task<Account> FindUserByEmail(string email);
        Task<Account> ResetPasswordToken(Account user);
        Task<Account> FindUserByPasswordResetToken(string token);
        Task<List<Account>> GetAllAccounts();
        Task<Account> GetAccountById(Guid id);
        Task<Account> Register(Account account);
        Task<Account> UpdateAccount(Account account);
    }
}
