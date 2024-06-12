using BusinessObjects;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly GenericDao<Account> _dao;
        public AccountRepository()
        {
            _dao = new GenericDao<Account>();
        }

        public Task<List<Account>> FindMany(
        Func<Account, bool> predicate,
        int page,
        int pageSize
    )
        {
            try
            {
                var users = _dao.GetQueryable().Where(predicate).Skip((page * pageSize) - 1).Take(pageSize).ToList();

                return Task.FromResult(users);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<Account?> FindOne(Func<Account, bool> predicate)
        {
            try
            {
                var result = _dao.GetQueryable().FirstOrDefault(predicate);
                return Task.FromResult<Account?>(result);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<Account> FindUserByEmail(string email)
        {
            var user = _dao.GetQueryable().FirstOrDefault(c => c.Email == email);
            return Task.FromResult((user == null) ? null : user);
        }

        public Task<Account> FindUserByPasswordResetToken(string token)
        {
            var user = _dao.GetQueryable().FirstOrDefault(c => c.PasswordResetToken == token);
            return Task.FromResult((user == null) ? null : user);
        }

        public async Task<Account> GetAccountById(Guid id)
        {
                var user = await _dao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == id);
                return user;
        }
        public async Task<List<Account>> GetAllAccounts()
        {
            var list = await _dao.GetQueryable().ToListAsync();
            return list;
        }

        public Task<Account> Register(Account account)
        {
            _dao.AddAsync(account);
            _dao.SaveChangesAsync();
            return Task.FromResult<Account>(account);
        }

        public Task ResetPassword(Guid uid, string password)
        {
            try
            {
                var user = _dao.GetQueryable().FirstOrDefault(c => c.AccountId == uid);
                if (user == null)
                {
                    throw new Exception();
                }
                else
                {
                    //user.PasswordSalt = password;
                    return Task.FromResult(user);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Account> ResetPasswordToken(Account user)
        {
            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            return await Task.FromResult(user);
        }

        public async Task<Account> UpdateAccount(Account account)
        {
            _dao.UpdateAsync(account);
            await _dao.SaveChangesAsync();
            return await Task.FromResult<Account>(account);
        }

        public string CreateRandomToken()
        {
            Random random = new Random();

            // Tạo một số ngẫu nhiên gồm 6 chữ số
            int randomNumber = random.Next(100000, 999999);
            return randomNumber.ToString();
        }
    }
}
