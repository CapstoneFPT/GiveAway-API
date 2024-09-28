using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
            IRechargeRepository rechargeRepository, ILogger<TransactionService> logger, IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
            _rechargeRepository = rechargeRepository;
            _logger = logger;
            _accountRepository = accountRepository;
        }
        public async Task<DotNext.Result<ExcelResponse, ErrorCode>> ExportTransactionsToExcel(ExportTransactionsRequest request)
        {
            try
            {
                Expression<Func<Transaction, bool>> predicate = t => true;
                if (request.StartDate != null)
                {
                    predicate = predicate.And(t => t.CreatedDate >= request.StartDate);
                }

                if (request.EndDate != null)
                {
                    predicate = predicate.And(t => t.CreatedDate <= request.EndDate);
                }

                if (request.MinAmount != null)
                {
                    predicate = predicate.And(t => t.Amount >= request.MinAmount);
                }

                if (request.MaxAmount != null)
                {
                    predicate = predicate.And(t => t.Amount <= request.MaxAmount);
                }

                if(request.ReceiverName != null)
                {
                    predicate = predicate.And(t => t.Receiver != null && EF.Functions.ILike(t.Receiver.Fullname, $"%{request.ReceiverName}%"));
                }

                if(request.SenderName != null)
                {
                    predicate = predicate.And(t => t.Sender != null && EF.Functions.ILike(t.Sender.Fullname, $"%{request.SenderName}%"));
                }
                
                if(request.PaymentMethods.Length > 0)
                {
                    predicate = predicate.And(t => request.PaymentMethods.Contains(t.PaymentMethod));
                }

                if(request.Types.Length > 0)
                {
                    predicate = predicate.And(t => request.Types.Contains(t.Type));
                }

                if(request.TransactionCode != null)
                {
                    predicate = predicate.And(t => EF.Functions.ILike(t.TransactionCode, $"%{request.TransactionCode}%"));
                }

                if (request.ShopId.HasValue)
                {
                    predicate = predicate.And(t => t.ShopId == request.ShopId);
                }

                var transactions = await _transactionRepository.GetQueryable()
                    .Where(predicate)
                    .Select(t => new
                    {
                        TransactionCode = t.TransactionCode,
                        t.CreatedDate,
                        t.Type,
                        t.Amount,
                        t.PaymentMethod,
                        ShopAddress = t.Shop != null ? t.Shop.Address : null
                    })
                    .ToListAsync();
                

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Transactions");

                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1"].Value = transactions.FirstOrDefault()?.ShopAddress != null ? $"Transactions Report for {transactions.FirstOrDefault()?.ShopAddress}" : "Transactions Report";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var headerStyle = worksheet.Cells["A3:F3"].Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.PatternType = ExcelFillStyle.Solid;
                headerStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                headerStyle.Font.Color.SetColor(System.Drawing.Color.White);

                var headers = new[] { 
                    "Transaction Code", 
                    "Date", 
                    "Type", 
                    "Amount", 
                    "Payment Method", 
                    "Shop Address"
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[3, i + 1].Value = headers[i];
                    worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var transaction in transactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.TransactionCode;
                    worksheet.Cells[row, 2].Value = transaction.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 3].Value = transaction.Type.ToString();
                    worksheet.Cells[row, 4].Value = transaction.Amount + " VND";
                    worksheet.Cells[row, 5].Value = transaction.PaymentMethod.ToString();
                    worksheet.Cells[row, 6].Value = transaction.ShopAddress ?? "N/A";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var content = await package.GetAsByteArrayAsync();

                return new DotNext.Result<ExcelResponse, ErrorCode>(new ExcelResponse
                {
                    Content = content,
                    FileName = $"Transactions_{DateTime.Now:yyyyMMdd}.xlsx"
                });
            }
            catch (Exception ex)
            {
                return new DotNext.Result<ExcelResponse, ErrorCode>(ErrorCode.ServerError);
            }
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
                    predicate = predicate.And(transaction => transaction.ShopId == transactionRequest.ShopId);
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
                    ProductCode = transaction.Order != null ? transaction.Order.OrderCode
                        : (transaction.ConsignSale!.ConsignSaleCode ??
                           transaction.Refund!.RefundCode ?? transaction.Recharge!.RechargeCode ??
                           transaction.AuctionDeposit!.DepositCode) ?? transaction.Withdraw!.WithdrawCode,
                    ConsignSaleId = transaction.ConsignSaleId,
                    PaymentMethod = transaction.PaymentMethod,
                    Amount = transaction.Amount,
                    CreatedDate = transaction.CreatedDate,
                    CustomerName = transaction.Order!.RecipientName != null
                        ? transaction.Order.RecipientName
                        : (transaction.ConsignSale!.ConsignorName ??
                           transaction.Withdraw!.Member.Fullname ?? transaction.Recharge!.Member.Fullname ??
                           transaction.AuctionDeposit!.Member.Fullname) ?? transaction.Refund!.OrderLineItem.Order.Member!.Fullname,
                    CustomerPhone = transaction.Order!.Phone != null
                        ? transaction.Order.Phone
                        : (transaction.ConsignSale!.Phone ??
                           transaction.Withdraw!.Member.Phone ?? transaction.Recharge!.Member.Phone ??
                           transaction.AuctionDeposit!.Member.Phone) ?? transaction.Refund!.OrderLineItem.Order.Member!.Phone,
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