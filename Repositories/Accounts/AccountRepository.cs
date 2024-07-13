﻿using BusinessObjects;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Utils;

namespace Repositories.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly GenericDao<Account?> _accountDao;

        public AccountRepository(GenericDao<Account?> genericDao)
        {
            _accountDao = genericDao;
        }

        public Task<List<Account?>> FindMany(
            Expression<Func<Account?, bool>> predicate,
            int page,
            int pageSize
        )
        {
            var result = _accountDao.GetQueryable()
                .Where(predicate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }

        public Task<Account?> FindOne(Expression<Func<Account?, bool>> predicate)
        {
            var result = _accountDao.GetQueryable().FirstOrDefaultAsync(predicate);

            return result;
        }

        public async Task<Account?> FindUserByEmail(string email)
        {
            var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.Email == email);
            return user;
        }

        public async Task<Account> FindUserByPasswordResetToken(string token)
        {
            var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.PasswordResetToken == token);
            return user;
        }

        public async Task<Account?> GetAccountById(Guid id)
        {
            var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == id);
            return user;
        }

        public async Task<List<Account?>> GetAllAccounts()
        {
            var list = await _accountDao.GetQueryable().ToListAsync();
            return list;
        }

        public async Task<Account?> Register(Account? account)
        {
            var result = await _accountDao.AddAsync(account);
            return result;
        }

        public async Task ResetPassword(Guid uid, string password)
        {
            var user = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == uid);
            if (user == null)
            {
                throw new AccountNotFoundException();
            }
        }

        public async Task<Account?> ResetPasswordToken(Account? user)
        {
            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(3);
            await _accountDao.UpdateAsync(user);
            return await Task.FromResult(user);
        }

        public async Task<Account?> UpdateAccount(Account? account)
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

        public Task<Account?> FindUserByPhone(string phone)
        {
            var user = _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.Phone == phone);

            return user;
        }

        public string? GetAdminAccount(string email, string password)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            // Check if the configuration key exists
            if (config.GetSection("AdminAccount").Exists())
            {
                string? emailJson = config["AdminAccount:Email"];
                string passwordJson = config["AdminAccount:Password"];

                // Check if both email and password match
                if (emailJson == email && passwordJson == password)
                {
                    return emailJson;
                }
            }

            return null;
        }

        public async Task<(List<TResponse> Items, int Page, int PageSize, int TotalCount)> GetAccounts<TResponse>(
            int? requestPage,
            int? requestPageSize,
            Expression<Func<Account, bool>>? predicate,
            Expression<Func<Account, TResponse>>? selector,
            bool isTracking)
        {
            var query = _accountDao.GetQueryable();

            if (isTracking) query = query.AsTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();

            var page = requestPage ?? -1;
            var pageSize = requestPageSize ?? -1;

            if (page > 0 && pageSize >= 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            List<TResponse> result;
            if (selector != null)
            {
                result = await query.Select(selector).ToListAsync();
            }
            else
            {
                result = await query.Cast<TResponse>().ToListAsync();
            }

            return (result, page, pageSize, totalCount);
        }
    }
}