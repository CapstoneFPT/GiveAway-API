using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Services.Transactions
{
    public interface ITransactionService
    {
        Task<Result<TransactionDetailResponse>> CreateTransactionFromVnPay(VnPaymentResponse vnPayResponse,
            TransactionType transactionType);

        Task CreateTransactionFromPoints(Order order, Guid requestMemberId, TransactionType transactionType);
    }
}
