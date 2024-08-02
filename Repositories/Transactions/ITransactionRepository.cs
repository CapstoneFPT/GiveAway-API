﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;

namespace Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<Transaction?> CreateTransaction(Transaction transaction);
        Task<(List<T> Items, int Page, int PageSize, int Total)> GetTransactionsProjection<T>(int? transactionRequestPage,
            int? transactionRequestPageSize, Expression<Func<Transaction, bool>>? predicate,
            Expression<Func<Transaction, T>>? selector);
        Task<GetTransactionsResponse> CreateTransactionRefund(Transaction transaction);
    }
}
