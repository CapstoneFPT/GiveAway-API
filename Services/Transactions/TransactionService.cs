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

        public TransactionService(ITransactionRepository transactionRepository, IOrderRepository orderRepository,
            IRechargeRepository rechargeRepository , ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
            _rechargeRepository = rechargeRepository;
            _logger = logger;
        }

        public async Task<Result<TransactionDetailResponse>> CreateTransactionFromVnPay(VnPaymentResponse vnPayResponse,
            TransactionType transactionType)
        {
            try
            {
                Transaction transaction = null!;
                switch (transactionType)
                {
                    case TransactionType.Purchase:

                        var order = await _orderRepository.GetSingleOrder(x =>
                            x.OrderId == new Guid(vnPayResponse.OrderId));
                        
                        if (order == null) throw new OrderNotFoundException();
                        
                        transaction = new Transaction()
                        {
                            OrderId = new Guid(vnPayResponse.OrderId),
                            CreatedDate = DateTime.UtcNow,
                            Amount = order.TotalPrice,
                            VnPayTransactionNumber = vnPayResponse.TransactionId,
                            MemberId = order.MemberId,
                            Type = transactionType
                        };
                        break;
                    case TransactionType.Recharge:
                        var recharge = await _rechargeRepository.GetQueryable()
                            .FirstOrDefaultAsync(x => x.RechargeId == new Guid(vnPayResponse.OrderId));
                        
                        if (recharge == null) throw new RechargeNotFoundException();
                        
                        transaction = new Transaction()
                        {
                            RechargeId = new Guid(vnPayResponse.OrderId),
                            CreatedDate = DateTime.UtcNow,
                            Amount = recharge.Amount,
                            VnPayTransactionNumber = vnPayResponse.TransactionId,
                            MemberId = recharge.MemberId,
                            Type = transactionType
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

        public Task CreateTransactionFromPoints(Order order, Guid requestMemberId, TransactionType transactionType)
        {
            var transaction = new Transaction
            {
                OrderId = order.OrderId,
                CreatedDate = DateTime.UtcNow,
                Amount = order.TotalPrice,
                MemberId = requestMemberId,
                Type = transactionType
            };
            return _transactionRepository.CreateTransaction(transaction);
        }

        public async Task<Result<PaginationResponse<TransactionResponse>>> GetAllTransaction(
            TransactionRequest transactionRequest)
        {
            try
            {
                Expression<Func<Transaction, bool>> predicate = transaction => true;
                if (transactionRequest.ShopId.HasValue)
                {
                    predicate = transaction => transaction.Order!.PurchaseType.Equals(PurchaseType.Offline);
                    // && transaction.Order!.OrderDetails.FirstOrDefault()!.IndividualFashionItem!.ShopId == transactionRequest.ShopId;
                }

                if (transactionRequest.TransactionType.HasValue)
                {
                    predicate = predicate.And(c => c.Type.Equals(transactionRequest.TransactionType));
                }

                Expression<Func<Transaction, TransactionResponse>> selector = transaction => new TransactionResponse()
                {
                    TransactionId = transaction.TransactionId,
                    TransactionType = transaction.Type,
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
                        : transaction.ConsignSale!.Phone
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