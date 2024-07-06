using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace Services.Transactions
{
    public interface ITransactionService
    {
        Task<object> CreateTransaction(string responseOrderId);
    }
}
