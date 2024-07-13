using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly GenericDao<Transaction> _transactionDao;

        public TransactionRepository(GenericDao<Transaction> transactionDao)
        {
            _transactionDao = transactionDao;
        }

        public async Task<Transaction?> CreateTransaction(Transaction transaction)
        {
            var result = await _transactionDao.AddAsync(transaction);
            return result;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int Total)> GetTransactions<T>(
            int? transactionRequestPage,
            int? transactionRequestPageSize, Expression<Func<Transaction, bool>>? predicate,
            Expression<Func<Transaction, T>>? selector, bool isTracking = false)
        {
            var query = _transactionDao.GetQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var total = await query.CountAsync();
            
            var page = transactionRequestPage ?? -1;
            var pageSize = transactionRequestPageSize ?? -1;
            
            if(page > 0 && pageSize >= 0)
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