using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using Dao;

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
    }
}