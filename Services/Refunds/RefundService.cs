using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.Accounts;
using Repositories.OrderLineItems;
using Repositories.Orders;
using Repositories.Refunds;
using Repositories.Transactions;
using Services.Emails;

namespace Services.Refunds
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly ILogger<RefundService> _logger;

        public RefundService(IRefundRepository refundRepository, IOrderRepository orderRepository,
            ITransactionRepository transactionRepository, IAccountRepository accountRepository,
            IEmailService emailService, IOrderLineItemRepository orderLineItemRepository, ILogger<RefundService> logger)
        {
            _refundRepository = refundRepository;
            _orderRepository = orderRepository;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _orderLineItemRepository = orderLineItemRepository;
            _logger = logger;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId,
            ApprovalRefundRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<RefundResponse>();
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var refund = await _refundRepository.GetSingleRefund(predicate);
            if (refund is null)
            {
                throw new RefundNotFoundException();
            }

            if (refund.RefundStatus != RefundStatus.Pending)
            {
                throw new StatusNotAvailableWithMessageException("This refund is not pending for approval");
            }
            if (request.Status != RefundStatus.Approved && request.Status != RefundStatus.Rejected)
            {
                throw new StatusNotAvailableWithMessageException("You can only Approve or Reject the refund");
            }

            var data = await _refundRepository.ApprovalRefundFromShop(refundId, request);
            await _emailService.SendEmailRefund(refundId);
            response.Data = data;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
        }

        public async Task<DotNext.Result<RefundResponse,ErrorCode>> GetRefundById(Guid refundId)
        {
            try
            {
                Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
                var queryable = _refundRepository.GetQueryable();

                var data = await _refundRepository.GetQueryable()
                    .Where(predicate)
                    .Select(refund => new RefundResponse()
                    {
                        ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode ?? string.Empty,
                        Description = refund.Description ?? string.Empty,
                        OrderCode = refund.OrderLineItem.Order.OrderCode,
                        CreatedDate = refund.CreatedDate,
                        RefundStatus = refund.RefundStatus,
                        RefundId = refund.RefundId,
                        CustomerEmail = refund.OrderLineItem.Order.Email ?? string.Empty,
                        CustomerName = refund.OrderLineItem.Order.Member != null
                            ? refund.OrderLineItem.Order.Member.Fullname
                            : string.Empty,
                        RecipientName = refund.OrderLineItem.Order.RecipientName! ?? string.Empty,
                        RefundAmount = refund.OrderLineItem.UnitPrice * refund.RefundPercentage / 100,
                        RefundPercentage = refund.RefundPercentage,
                        UnitPrice = refund.OrderLineItem.UnitPrice,
                        ItemImages = refund.OrderLineItem.IndividualFashionItem.Images.Select(c => c.Url).ToArray(),
                        ImagesForCustomer = refund.Images.Select(x => x.Url).ToArray(),
                        ItemName = refund.OrderLineItem.IndividualFashionItem.MasterItem.Name,
                        OrderLineItemId = refund.OrderLineItemId,
                        ResponseFromShop = refund.ResponseFromShop,
                        CustomerPhone = refund.OrderLineItem.Order.Phone ?? string.Empty,
                    }).FirstOrDefaultAsync();

                if (data is null)
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.NotFound);
                }

                return new Result<RefundResponse, ErrorCode>(data);
            }
            catch (Exception e)
            {
                return new Result<RefundResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<PaginationResponse<RefundResponse>> GetAllRefunds(
            RefundRequest request)
        {
            Expression<Func<Refund, bool>> predicate = x => true;
            Expression<Func<Refund, RefundResponse>> selector = item => new
                RefundResponse()
                {
                    ItemCode = item.OrderLineItem.IndividualFashionItem.ItemCode ?? string.Empty,
                    Description = item.Description ?? string.Empty,
                    OrderCode = item.OrderLineItem.Order.OrderCode,
                    CreatedDate = item.CreatedDate,
                    RefundStatus = item.RefundStatus,
                    RefundId = item.RefundId,
                    CustomerEmail = item.OrderLineItem.Order.Email! ?? string.Empty,
                    CustomerName = item.OrderLineItem.Order.RecipientName! ?? string.Empty,
                    RefundAmount = item.OrderLineItem.UnitPrice * item.RefundPercentage / 100,
                    RefundPercentage = item.RefundPercentage,
                    UnitPrice = item.OrderLineItem.UnitPrice,
                    ItemImages = item.OrderLineItem.IndividualFashionItem.Images.Select(c => c.Url).ToArray(),
                    ImagesForCustomer = item.Images.Select(x => x.Url).ToArray(),
                    ItemName = item.OrderLineItem.IndividualFashionItem.MasterItem.Name,
                    OrderLineItemId = item.OrderLineItemId,
                    ResponseFromShop = item.ResponseFromShop,
                    CustomerPhone = item.OrderLineItem.Order.Phone! ?? string.Empty,
                };
            (List<RefundResponse> Items, int Page, int PageSize, int TotalCount) result =
                new ValueTuple<List<RefundResponse>, int, int, int>();

            if (request.MemberId.HasValue)
            {
                predicate = predicate.And(item => item.OrderLineItem.Order.MemberId == request.MemberId);
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item =>
                    item.OrderLineItem.IndividualFashionItem.MasterItem.ShopId == request.ShopId);
            }

            if (request.Status != null)
            {
                predicate = predicate.And(item => request.Status.Contains(item.RefundStatus));
            }

            if (request.PreviousTime.HasValue)
            {
                predicate = predicate.And(item => item.CreatedDate <= request.PreviousTime);
            }

            if (request.CustomerEmail != null)
            {
                predicate = predicate.And(item => item.OrderLineItem.Order.Email == request.CustomerEmail);
            }

            if (request.CustomerPhone != null)
            {
                predicate = predicate.And(item => item.OrderLineItem.Order.Phone == request.CustomerPhone);
            }

            if (request.CustomerName != null)
            {
                predicate = predicate.And(item => item.OrderLineItem.Order.RecipientName == request.CustomerName);
            }

            if (request.OrderCode != null)
            {
                predicate = predicate.And(item => item.OrderLineItem.Order.OrderCode == request.OrderCode);
            }

            if (request.ItemCode != null)
            {
                predicate = predicate.And(item =>
                    item.OrderLineItem.IndividualFashionItem.ItemCode == request.ItemCode);
            }

            if (request.ItemName != null)
            {
                predicate = predicate.And(item =>
                    item.OrderLineItem.IndividualFashionItem.MasterItem.Name == request.ItemName);
            }

            result = await _refundRepository.GetRefundProjections(request.PageNumber, request.PageSize,
                predicate,
                selector);


            return new PaginationResponse<RefundResponse>()
            {
                Items = result.Items!,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }

        private async Task<decimal> CalculateRefundAmount(Guid orderId, Guid orderLineItemId, int percentageRefund)
        {
            var orderQuery = _orderRepository.GetQueryable();
            var orderLineItemQuery = _orderLineItemRepository.GetQueryable();
            var lineItemsCount = await orderQuery.Where(x=> x.OrderId == orderId)
                .Include(x => x.OrderLineItems)
                .CountAsync();
            var order = await orderQuery.FirstOrDefaultAsync(x=> x.OrderId == orderId);
            var orderLineItem = await orderLineItemQuery.FirstOrDefaultAsync(x => x.OrderLineItemId == orderLineItemId);

            if (order == null || orderLineItem == null)
            {
                throw new UnableToCalculateRefundAmountException("Order or OrderLineItem not found");
            }

            var discount = order.Discount;
            var shippingFee = order.ShippingFee;
            var unitPrice = orderLineItem.UnitPrice;
            
            
            _logger.LogInformation("Discount: {Discount}, ShippingFee: {ShippingFee}, UnitPrice: {UnitPrice}", discount, shippingFee, unitPrice);
            _logger.LogInformation("PercentageRefund: {PercentageRefund}", percentageRefund);
            _logger.LogInformation("LineItemsCount: {LineItemsCount}", lineItemsCount);
            
            var refundAmount = Math.Round((unitPrice - discount/lineItemsCount + shippingFee/lineItemsCount) * percentageRefund / 100);
            
            _logger.LogInformation("RefundAmount: {RefundAmount}", refundAmount);
            
            return refundAmount;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> ConfirmReceivedAndRefund(Guid refundId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<RefundResponse>();
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var refund = await _refundRepository.GetSingleRefund(predicate);
            if (refund == null)
            {
                throw new RefundNotFoundException();
            }

            var order = await _orderRepository.GetSingleOrder(c =>
                c.OrderLineItems.Select(orderLineItem => orderLineItem.OrderLineItemId).Contains(refund.OrderLineItemId));
            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            if (!refund.RefundStatus.Equals(RefundStatus.Approved))
            {
                response.ResultStatus = ResultStatus.Error;
                response.Messages = new[] { "This refund is not available to confirm" };
                return response;
            }

            var refundResponse = await _refundRepository.ConfirmReceivedAndRefund(refundId);

            var member = await _accountRepository.GetAccountById(order.MemberId!.Value);
            if (member == null)
            {
                throw new AccountNotFoundException();
            }

            var refundAmount = await CalculateRefundAmount(order.OrderId, refund.OrderLineItemId, refund.RefundPercentage!.Value);
            member.Balance += refundAmount;
            await _accountRepository.UpdateAccount(member);

            var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
            if (admin == null)
                throw new AccountNotFoundException();
            admin.Balance -= refundAmount;
            await _accountRepository.UpdateAccount(admin);

            Transaction refundTransaction = new Transaction()
            {
                Amount = refundAmount,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.Refund,
                RefundId = refundId,
                MemberId = order.MemberId
            };
            await _transactionRepository.CreateTransactionRefund(refundTransaction);
            response.Data = refundResponse;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = new[] { "Confirm item is received and refund to customer successfully" };
            return response;
        }

        public async Task<Result<RefundResponse,ErrorCode>> CreateRefundByShop(Guid shopId, CreateRefundByShopRequest request)
        {
            var refund = new Refund()
            {
                Description = request.Description,
                CreatedDate = DateTime.UtcNow,
                OrderLineItemId = request.OrderLineItemId,
                RefundStatus = RefundStatus.Approved,
                RefundPercentage = request.RefundPercentage,
                ResponseFromShop = "We accepted refund request at shop"
            };
            await _refundRepository.CreateRefund(refund);
            var result = await GetRefundById(refund.RefundId);
            return result.IsSuccessful ? result : new Result<RefundResponse, ErrorCode>(result.Error);
        }

        public async Task<Result<RefundResponse,ErrorCode>> CancelRefund(Guid refundId)
        {

            try
            {
                Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
                var refund = await _refundRepository.GetSingleRefund(predicate);
                if (refund == null)
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.NotFound);
                }

                if (refund.RefundStatus != RefundStatus.Pending && refund.RefundStatus != RefundStatus.Approved)
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.RefundStatusNotAvailable);
                }
                refund.RefundStatus = RefundStatus.Cancelled;
                refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Sold;
                await _refundRepository.UpdateRefund(refund);
            
                var data = new RefundResponse()
                {
                    RefundId = refund.RefundId,
                    OrderLineItemId = refund.OrderLineItemId,
                    RefundStatus = refund.RefundStatus,
                    CreatedDate = refund.CreatedDate,
                    ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode
                };

                return data;
            }
            catch (Exception e)
            {
                return new Result<RefundResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<Result<RefundResponse, ErrorCode>> UpdateRefund(Guid refundId, UpdateRefundRequest request)
        {
            try
            {
                Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
                var refund = await _refundRepository.GetSingleRefund(predicate);
                if (refund == null)
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.NotFound);
                }
                if (refund.RefundStatus != RefundStatus.Pending || request.Description!.Trim().IsNullOrEmpty())
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.RefundStatusNotAvailable);
                }

                if (request.RefundImages.Length == 0)
                {
                    return new Result<RefundResponse, ErrorCode>(ErrorCode.MissingFeature);
                }
                refund.Description = request.Description ?? refund.Description;
                refund.Images.Clear();
                refund.Images = request.RefundImages.Select(imageUrl => new Image()
                {
                    RefundId = refund.RefundId,
                    Url = imageUrl,
                    CreatedDate = DateTime.UtcNow
                }).ToList();
                await _refundRepository.UpdateRefund(refund);
                return new RefundResponse()
                {
                    RefundId = refund.RefundId,
                    OrderLineItemId = refund.OrderLineItemId,
                    RefundStatus = refund.RefundStatus,
                    CreatedDate = refund.CreatedDate,
                    ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode,
                    ImagesForCustomer = refund.Images.Select(c => c.Url).ToArray()
                };
            }
            catch (Exception e)
            {
                return new Result<RefundResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }
    }

    
}