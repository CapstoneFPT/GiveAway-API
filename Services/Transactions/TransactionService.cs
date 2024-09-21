using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Accounts;
using Repositories.Orders;
using Repositories.Recharges;
using Repositories.Transactions;
using Transaction = BusinessObjects.Entities.Transaction;

namespace Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRechargeRepository _rechargeRepository;
        private readonly ILogger<TransactionService> _logger;
        private readonly IAccountRepository _accountRepository;
        public TransactionService(ITransactionRepository transactionRepository, IOrderRepository orderRepository,
            IRechargeRepository rechargeRepository , ILogger<TransactionService> logger, IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
            _rechargeRepository = rechargeRepository;
            _logger = logger;
            _accountRepository = accountRepository;
        }

        public async Task<Result<TransactionDetailResponse>> CreateTransactionFromVnPay(VnPaymentResponse vnPayResponse,
            TransactionType transactionType)
        {
            try
            {
                var admin = await _accountRepository.FindOne(c => c.Role == Roles.Admin);
                Transaction transaction = null!;
                switch (transactionType)
                {
                    case TransactionType.Purchase:

                        var order = await _orderRepository.GetSingleOrder(x =>
                            x.OrderId == new Guid(vnPayResponse.OrderId));
                        
                        var memberAcc = await _accountRepository.FindOne(x => x.AccountId == order.MemberId);
                        if (order == null) throw new OrderNotFoundException();
                        
                        transaction = new Transaction()
                        {
                            OrderId = new Guid(vnPayResponse.OrderId),
                            CreatedDate = DateTime.UtcNow,
                            Amount = order.TotalPrice,
                            VnPayTransactionNumber = vnPayResponse.TransactionId,
                            SenderId = order.MemberId,
                            SenderBalance = memberAcc.Balance,
                            ReceiverId = admin.AccountId,
                            ReceiverBalance = admin.Balance,
                            Type = transactionType,
                            PaymentMethod = PaymentMethod.Banking
                        };
                        break;
                    case TransactionType.AddFund:
                        var recharge = await _rechargeRepository.GetQueryable()
                            .FirstOrDefaultAsync(x => x.RechargeId == new Guid(vnPayResponse.OrderId));
                        
                        if (recharge == null) throw new RechargeNotFoundException();

                        var member = await _accountRepository.FindOne(x => x.AccountId == recharge.MemberId);
                        transaction = new Transaction()
                        {
                            RechargeId = new Guid(vnPayResponse.OrderId),
                            CreatedDate = DateTime.UtcNow,
                            Amount = recharge.Amount,
                            VnPayTransactionNumber = vnPayResponse.TransactionId,
                            ReceiverId = recharge.MemberId,
                            ReceiverBalance = member.Balance,
                            SenderId = admin.AccountId,
                            SenderBalance = admin.Balance,
                            Type = transactionType,
                            PaymentMethod = PaymentMethod.Banking
                        };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
                }


                var createTransactionResult = await _transactionRepository.CreateTransaction(transaction);

                return new Result<TransactionDetailResponse>()
                {
                    Data = new TransactionDetailResponse
                    {
                        TransactionId = createTransactionResult!.TransactionId
                    },
                    ResultStatus = ResultStatus.Success
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating transaction {TransactionId}", vnPayResponse.TransactionId);
                return new Result<TransactionDetailResponse>()
                {
                    ResultStatus = ResultStatus.Error
                };
            }
        }

        public async Task CreateTransactionFromPoints(Order order, Guid requestMemberId, TransactionType transactionType)
        {
            var admin = await _accountRepository.FindOne(c => c.Role == Roles.Admin);
            var member = await _accountRepository.FindOne(x => x.AccountId == requestMemberId);
            var transaction = new Transaction
            {
                OrderId = order.OrderId,
                CreatedDate = DateTime.UtcNow,
                Amount = order.TotalPrice,
                SenderId = requestMemberId,
                SenderBalance = member.Balance,
                ReceiverId = admin.AccountId,
                ReceiverBalance = admin.Balance,
                Type = transactionType,
                PaymentMethod = PaymentMethod.Point
            }; 
            await _transactionRepository.CreateTransaction(transaction);
        }

        public async Task<Result<PaginationResponse<TransactionResponse>>> GetAllTransaction(
            TransactionRequest transactionRequest)
        {
            try
            {
                Expression<Func<Transaction, bool>> predicate = transaction => true;
                if (transactionRequest.ShopId.HasValue)
                {
                    predicate = predicate.And(transaction =>  transaction.ShopId == transactionRequest.ShopId);
                }

                if (transactionRequest.TransactionType.HasValue)
                {
                    predicate = predicate.And(c => c.Type.Equals(transactionRequest.TransactionType));
                }

                Expression<Func<Transaction, TransactionResponse>> selector = transaction => new TransactionResponse()
                {
                    TransactionId = transaction.TransactionId,
                    TransactionType = transaction.Type,
                    TransactionCode = transaction.TransactionCode,
                    OrderId = transaction.OrderId,
                    OrderCode = transaction.Order != null ? transaction.Order.OrderCode : null,
                    ConsignSaleId = transaction.ConsignSaleId,
                    ConsignSaleCode = transaction.ConsignSale != null ? transaction.ConsignSale.ConsignSaleCode : null,
                    Amount = transaction.Amount,
                    CreatedDate = transaction.CreatedDate,
                    CustomerName = transaction.Order!.RecipientName != null
                        ? transaction.Order!.RecipientName
                        : transaction.ConsignSale!.ConsignorName,
                    CustomerPhone = transaction.Order!.Phone != null
                        ? transaction.Order!.Phone
                        : transaction.ConsignSale!.Phone,
                    ShopId = transaction.ShopId
                };
                Expression<Func<Transaction, DateTime>> orderBy = transaction => transaction.CreatedDate;
                (List<TransactionResponse> Items, int Page, int PageSize, int Total) result =
                    await _transactionRepository.GetTransactionsProjection<TransactionResponse>(transactionRequest.Page,
                        transactionRequest.PageSize, predicate, orderBy, selector);
                return new Result<PaginationResponse<TransactionResponse>>()
                {
                    Data = new PaginationResponse<TransactionResponse>()
                    {
                        Items = result.Items,
                        PageNumber = result.Page,
                        PageSize = result.PageSize,
                        TotalCount = result.Total
                    },
                    Messages = new[] { "Result with " + result.Total + " transaction" },
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

    public class RechargeNotFoundException : Exception
    {
    }
}