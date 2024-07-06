using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;

namespace Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<Transaction?> CreateTransaction(Transaction transaction);
    }
}
