using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using BusinessObjects.Dtos.Commons;
using Repositories.Transactions;
using Transaction = BusinessObjects.Entities.Transaction;

namespace Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public Task<object> CreateTransaction(VnPaymentResponseModel responseOrderId)
        {
            try
            {
                var transaction = new Transaction()
                {
                    OrderId = new Guid(responseOrderId.OrderId),
                    CreatedDate = DateTime.UtcNow,
                    Amount = responseOrderId.Amount,
                    Status = responseOrderId.Success ? TransactionStatus.Committed : TransactionStatus.Aborted
                };
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}