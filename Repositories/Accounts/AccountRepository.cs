using BusinessObjects;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly GenericDao<Account> _accountDao;
        public AccountRepository(GenericDao<Account> genericDao)
        {
            _accountDao = genericDao;
        }

        public Task<List<Account>> FindMany(
        Func<Account, bool> predicate,
        int page,
        int pageSize
    )
        {
            try
            {
                var users = _accountDao.GetQueryable().Where(predicate).Skip((page * pageSize) - 1).Take(pageSize).ToList();

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
                var result = _accountDao.GetQueryable().FirstOrDefault(predicate);
                return Task.FromResult<Account?>(result);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<Account> FindUserByEmail(string email)
        {
            var user = _accountDao.GetQueryable().FirstOrDefault(c => c.Email == email);
            return Task.FromResult((user == null) ? null : user);
        }

        public async Task<Account> FindUserByPasswordResetToken(string token)
        {
            var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.PasswordResetToken == token);
            return user;
        }

        public async Task<Account> GetAccountById(Guid id)
        {
                var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == id);
                return user;
        }
        public async Task<List<Account>> GetAllAccounts()
        {
            var list = await _accountDao.GetQueryable().ToListAsync();
            return list;
        }

        public async Task<Account> Register(Account account)
        {
            var result = await _accountDao.AddAsync(account);
            return result;
        }

        public Task ResetPassword(Guid uid, string password)
        {
            try
            {
                var user = _accountDao.GetQueryable().FirstOrDefault(c => c.AccountId == uid);
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
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(3);
            await _accountDao.UpdateAsync(user);
            return await Task.FromResult(user);
        }

        public async Task<Account> UpdateAccount(Account account)
        {
            await _accountDao.UpdateAsync(account);
            return await Task.FromResult<Account>(account);
        }

        public string CreateRandomToken()
        {
            Random random = new Random();

            // Tạo một số ngẫu nhiên gồm 6 chữ số
            int randomNumber = random.Next(100000, 999999);
            return randomNumber.ToString();
        }

        public Task<Account> FindUserByPhone(string phone)
        {
            var user = _accountDao.GetQueryable().FirstOrDefault(c => c.Phone == phone);
            return Task.FromResult((user == null) ? null : user);
        }

        public async Task<string> GetAdminAccount(string email, string password)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            // Check if the configuration key exists
            if (config.GetSection("AdminAccount").Exists())
            {
                string emailJson = config["AdminAccount:Email"];
                string passwordJson = config["AdminAccount:Password"];

                // Check if both email and password match
                if (emailJson == email && passwordJson == password)
                {
                    return emailJson;
                }
            }

            return null;
        }
    }
}
