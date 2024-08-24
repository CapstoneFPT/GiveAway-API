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

        public TransactionRepository(GiveAwayDbContext giveAwayDbContext, IMapper mapper)
        {
            _giveAwayDbContext = giveAwayDbContext;
            _mapper = mapper;
        }

        public async Task<Transaction?> CreateTransaction(Transaction transaction)
        {
            var result = await GenericDao<Transaction>.Instance.AddAsync(transaction);
            return result;
        }

        public async Task<GetTransactionsResponse> CreateTransactionRefund(Transaction transaction)
        {
            var result = await GenericDao<Transaction>.Instance.AddAsync(transaction);
            return _mapper.Map<GetTransactionsResponse>(result);
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
            
            var page = transactionRequestPage ?? -1;
            var pageSize = transactionRequestPageSize ?? -1;
            
            if(page > 0 && pageSize >= 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }
            
            List<T> result;
            if (selector != null)
            {
                result = await query.OrderByDescending(orderBy).Select(selector).ToListAsync();
            }
            else
            {
                result = await query.OrderByDescending(orderBy).Cast<T>().ToListAsync();
            }
            
            return (result, page, pageSize, total);
            
        }
    }
}