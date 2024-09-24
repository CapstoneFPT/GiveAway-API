using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly GiveAwayDbContext _giveAwayDbContext;
        private readonly IMapper _mapper;
        private const string Prefix = "TRX";
        private static Random _random = new();

        public TransactionRepository(GiveAwayDbContext giveAwayDbContext, IMapper mapper)
        {
            _giveAwayDbContext = giveAwayDbContext;
            _mapper = mapper;
        }

        public IQueryable<Transaction> GetQueryable()
        {
            return _giveAwayDbContext.Transactions.AsQueryable();
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

        public async Task<Transaction?> CreateTransaction(Transaction transaction)
        {
            transaction.TransactionCode = await GenerateUniqueString();
            var result = await GenericDao<Transaction>.Instance.AddAsync(transaction);
            return result;
        }

        

        public async Task<(List<T> Items, int Page, int PageSize, int Total)> GetTransactionsProjection<T>(
            int? transactionRequestPage,
            int? transactionRequestPageSize, Expression<Func<Transaction, bool>>? predicate,
            Expression<Func<Transaction, DateTime>> orderBy,
            Expression<Func<Transaction, T>>? selector)
        {
            var query = _giveAwayDbContext.Transactions.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var total = await query.CountAsync();
            
            query = query.OrderByDescending(orderBy);
            
            var page = transactionRequestPage ?? -1;
            var pageSize = transactionRequestPageSize ?? -1;

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
}