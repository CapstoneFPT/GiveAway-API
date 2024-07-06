using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;

namespace Services.Transactions
{
    public interface ITransactionService
    {
        Task<Result<TransactionDetailResponse>> CreateTransaction(VnPaymentResponse vnPayResponse,
            TransactionType transactionType);
    }
}
