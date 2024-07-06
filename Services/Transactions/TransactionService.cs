using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using Repositories.Orders;
using Repositories.Transactions;
using Transaction = BusinessObjects.Entities.Transaction;

namespace Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOrderRepository _orderRepository;

        public TransactionService(ITransactionRepository transactionRepository, IOrderRepository orderRepository)
        {
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Result<TransactionDetailResponse>> CreateTransaction(VnPaymentResponse vnPayResponse,
            TransactionType transactionType)
        {
            try
            {
                var order = await _orderRepository.GetSingleOrder(x => x.OrderId == new Guid(vnPayResponse.OrderId));

                if (order == null) throw new Exception("Order not found");
                var transaction = new Transaction()
                {
                    OrderId = new Guid(vnPayResponse.OrderId),
                    CreatedDate = DateTime.UtcNow,
                    Amount = order.TotalPrice,
                    TransactionNumber = vnPayResponse.TransactionId,
                    MemberId = order.MemberId,
                    Type = transactionType 
                };

                var createTransactionResult = await _transactionRepository.CreateTransaction(transaction);

                return new Result<TransactionDetailResponse>()
                {
                    Data = new TransactionDetailResponse
                    {
                        TransactionId = createTransactionResult.TransactionId
                    },
                    ResultStatus = ResultStatus.Success
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