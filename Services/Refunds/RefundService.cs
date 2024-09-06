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
using LinqKit;
using Microsoft.EntityFrameworkCore;
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

        public RefundService(IRefundRepository refundRepository, IOrderRepository orderRepository,
            ITransactionRepository transactionRepository, IAccountRepository accountRepository,
            IEmailService emailService)
        {
            _refundRepository = refundRepository;
            _orderRepository = orderRepository;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _emailService = emailService;
        }

        public async Task<Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId,
            ApprovalRefundRequest request)
        {
            var response = new Result<RefundResponse>();
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var refund = await _refundRepository.GetSingleRefund(predicate);
            if (refund is null)
            {
                throw new RefundNoFoundException();
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
            await _emailService.SendEmailRefund(data);
            response.Data = data;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
        }

        public async Task<Result<RefundResponse>> GetRefundById(Guid refundId)
        {
            var response = new Result<RefundResponse>();
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var item = await _refundRepository.GetSingleRefund(predicate);
            if (item is null)
            {
                throw new RefundNoFoundException();
            }

            response.Data = new RefundResponse()
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
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
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
                predicate = predicate.And(item => item.OrderLineItem.IndividualFashionItem.ItemCode == request.ItemCode);
            }
            if (request.ItemName != null)
            {
                predicate = predicate.And(item => item.OrderLineItem.IndividualFashionItem.MasterItem.Name == request.ItemName);
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


        public async Task<Result<RefundResponse>> ConfirmReceivedAndRefund(Guid refundId)
        {
            var response = new Result<RefundResponse>();
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var refund = await _refundRepository.GetSingleRefund(predicate);
            if (refund == null)
            {
                throw new RefundNoFoundException();
            }

            var order = await _orderRepository.GetSingleOrder(c =>
                c.OrderLineItems.Select(c => c.OrderLineItemId).Contains(refund.OrderLineItemId));
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

            var refundAmount = refund.OrderLineItem.UnitPrice * refund.RefundPercentage / 100;
            member.Balance += refundAmount!.Value;
            await _accountRepository.UpdateAccount(member);

            var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
            if (admin == null)
                throw new AccountNotFoundException();
            admin.Balance -= refundAmount.Value;
            await _accountRepository.UpdateAccount(admin);

            Transaction refundTransaction = new Transaction()
            {
                Amount = refundAmount.Value,
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

        public async Task<Result<RefundResponse>> CreateRefundByShop(Guid shopId, CreateRefundByShopRequest request)
        {
            var response = new Result<RefundResponse>();
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
            return await GetRefundById(refund.RefundId);
        }

        public async Task<Result<RefundResponse>> CancelRefund(Guid refundId)
        {
            Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
            var refund = await _refundRepository.GetSingleRefund(predicate);
            if (refund == null)
            {
                throw new RefundNoFoundException();
            }

            if (refund.RefundStatus != RefundStatus.Rejected && refund.RefundStatus == RefundStatus.Approved)
            {
                throw new StatusNotAvailableWithMessageException("This refund has status can not be cancelled");
            }
            refund.RefundStatus = RefundStatus.Cancelled;
            refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Sold;
            await _refundRepository.UpdateRefund(refund);
            return new Result<RefundResponse>()
            {
                Data = new RefundResponse()
                {
                    RefundId = refund.RefundId,
                    OrderLineItemId = refund.OrderLineItemId,
                    RefundStatus = refund.RefundStatus,
                    CreatedDate = refund.CreatedDate,
                    ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode
                }
            };
        }
    }
}