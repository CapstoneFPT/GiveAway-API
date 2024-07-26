using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Accounts
{
    public interface IAccountRepository
    {
        Task<List<Account?>> FindMany(Expression<Func<Account?, bool>> predicate,
            int page,
            int pageSize);
        Task<Account?> FindOne(Expression<Func<Account?, bool>> predicate);
        Task ResetPassword(Guid uid, string password);
        Task<Account?> FindUserByEmail(string email);
        Task<Account?> ResetPasswordToken(Account? user);
        Task<Account> FindUserByPasswordResetToken(string token);
        Task<List<Account?>> GetAllAccounts();
        Task<Account?> GetAccountById(Guid id, bool isTracking = false);
        Task<Account?> Register(Account? account);
        Task<Account?> UpdateAccount(Account? account);
        string CreateRandomToken();
        Task<Account?> FindUserByPhone(string phone);
        string? GetAdminAccount(string email, string password);
        Task<(List<TResponse> Items, int Page, int PageSize, int TotalCount)> GetAccounts<TResponse>(int? requestPage, int? requestPageSize, Expression<Func<Account, bool>>? predicate, Expression<Func<Account, TResponse>>? selector, bool isTracking);
        Task<Member?> GetMemberById(Guid id);
        Task UpdateMemberAccount(Member member);
    }
}
