using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.OrderLineItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.FashionItems;
using Repositories.Orders;
using Repositories.Refunds;

namespace Services.OrderLineItems
{
    public class OrderLineItemService : IOrderLineItemService
    {
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly ILogger<OrderLineItemService> _logger;
        private readonly IRefundRepository _refundRepository;

        public OrderLineItemService(IOrderLineItemRepository orderLineItemRepository,
            IFashionItemRepository fashionItemRepository, ILogger<OrderLineItemService> logger,
            IRefundRepository refundRepository)
        {
            _orderLineItemRepository = orderLineItemRepository;
            _fashionItemRepository = fashionItemRepository;
            _logger = logger;
            _refundRepository = refundRepository;
        }

        public async Task<DotNext.Result<OrderLineItemDetailedResponse, ErrorCode>> GetOrderLineItemById(Guid orderId)
        {
            var query = _orderLineItemRepository.GetQueryable();

            Expression<Func<OrderLineItem, bool>> predicate = item => item.OrderId == orderId;
            Expression<Func<OrderLineItem, OrderLineItemDetailedResponse>> selector = item =>
                new OrderLineItemDetailedResponse()
                {
                    OrderLineItemId = item.OrderLineItemId,
                    CreatedDate = item.CreatedDate,
                    RefundExpirationDate = item.RefundExpirationDate,
                    Condition = item.IndividualFashionItem.Condition,
                    Quantity = item.Quantity,
                    CategoryName = item.IndividualFashionItem.MasterItem.Category.Name,
                    ItemBrand =item.IndividualFashionItem.MasterItem.Brand
                        ?? "N/A",
                    ItemCode = item.IndividualFashionItem.ItemCode,
                    ItemColor = item.IndividualFashionItem.Color,
                    UnitPrice = item.UnitPrice,
                    ItemGender = item.IndividualFashionItem.MasterItem.Gender,
                    ItemImage = item.IndividualFashionItem.Images.Select(x => x.Url).ToList(),
                    ItemName = item.IndividualFashionItem.MasterItem.Name,
                    ItemNote = item.IndividualFashionItem.Note,
                    ItemSize = item.IndividualFashionItem.Size,
                    ItemStatus = item.IndividualFashionItem.Status,
                    ItemType = item.IndividualFashionItem.Type,
                    OrderCode = item.Order.OrderCode,
                    PaymentDate = item.PaymentDate,
                    ShopAddress = item.IndividualFashionItem.MasterItem.Shop.Address,
                    ShopId = item.IndividualFashionItem.MasterItem.ShopId,
                };
            try
            {
                var result = await query
                    .Include(x => x.IndividualFashionItem)
                    .ThenInclude(x => x.Images)
                    .Include(x => x.IndividualFashionItem)
                    
                    .ThenInclude(x => x.MasterItem)
                    .ThenInclude(x => x.Shop)
                    .Include(x => x.Order)
                    .Where(predicate)
                    .Select(selector)
                    .FirstOrDefaultAsync();


                if (result == null) return new Result<OrderLineItemDetailedResponse, ErrorCode>(ErrorCode.NotFound);

                return new Result<OrderLineItemDetailedResponse, ErrorCode>(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetOrderLineItemById error");
                return new Result<OrderLineItemDetailedResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<DotNext.Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>>
            GetOrderLineItemsByOrderId(
                Guid orderId, OrderLineItemRequest request)
        {
            var query = _orderLineItemRepository.GetQueryable();

            Expression<Func<OrderLineItem, bool>> predicate = orderLineItem => orderLineItem.OrderId == orderId;
            Expression<Func<OrderLineItem, OrderLineItemListResponse>> selector = orderLineItem =>
                new OrderLineItemListResponse()
                {
                    OrderLineItemId = orderLineItem.OrderLineItemId,
                    CreatedDate = orderLineItem.CreatedDate,
                    RefundExpirationDate = orderLineItem.RefundExpirationDate,
                    Condition = orderLineItem.IndividualFashionItem.Condition,
                    ItemBrand = orderLineItem.IndividualFashionItem.MasterItem.Brand,
                    ItemCode = orderLineItem.IndividualFashionItem.ItemCode,
                    ItemColor = orderLineItem.IndividualFashionItem.Color,
                    Quantity = orderLineItem.Quantity,
                    CategoryName = orderLineItem.IndividualFashionItem.MasterItem.Category.Name,
                    ItemNote = orderLineItem.IndividualFashionItem.Note,
                    ItemSize = orderLineItem.IndividualFashionItem.Size,
                    ItemImage = orderLineItem.IndividualFashionItem.Images.Select(x => x.Url).ToList(),
                    ItemName = orderLineItem.IndividualFashionItem.MasterItem.Name,
                    UnitPrice = orderLineItem.UnitPrice,
                    ItemGender = orderLineItem.IndividualFashionItem.MasterItem.Gender,
                    ItemStatus = orderLineItem.IndividualFashionItem.Status,
                    ItemType = orderLineItem.IndividualFashionItem.Type,
                    OrderCode = orderLineItem.Order.OrderCode,
                    PaymentDate = orderLineItem.PaymentDate,
                    ShopAddress = orderLineItem.IndividualFashionItem.MasterItem.Shop.Address,
                    ShopId = orderLineItem.IndividualFashionItem.MasterItem.ShopId,
                };

            try
            {
                query = query
                    .Include(x => x.IndividualFashionItem)
                    .ThenInclude(x => x.Images)
                    .Include(x => x.IndividualFashionItem)
                    
                    .ThenInclude(x => x.MasterItem)
                    .ThenInclude(x => x.Shop)
                    .Include(x => x.Order);

                if (request.ShopId.HasValue)
                    predicate = predicate.And(x =>
                        x.IndividualFashionItem.MasterItem.ShopId == request.ShopId.Value);

                query = query.Where(predicate);
                var page = request.PageNumber ?? -1;
                var pageSize = request.PageSize ?? -1;

                if (page > 0 && pageSize > 0)
                {
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }


                var totalCount = await query.CountAsync();

                var items = await query.Select(selector).ToListAsync();

                var response = new PaginationResponse<OrderLineItemListResponse>()
                {
                    Items = items,
                    PageNumber = request.PageNumber ?? -1,
                    PageSize = request.PageSize ?? -1,
                    TotalCount = totalCount
                };

                return new Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>(response);
            }
            catch (Exception e)
            {
                return new Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> RequestRefundToShop(
            CreateRefundRequest refundRequest)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<RefundResponse>();

            var orderDetail =
                await _orderLineItemRepository.GetOrderLineItemById(refundRequest.OrderLineItemId);
            if (orderDetail is null)
            {
                throw new OrderNotFoundException();
            }


            if (orderDetail.RefundExpirationDate < DateTime.UtcNow)
            {
                throw new RefundExpiredException("There are items that ran out refund expiration");
            }

            
            if (orderDetail.IndividualFashionItem.Status != FashionItemStatus.Refundable)
            {
                throw new StatusNotAvailableWithMessageException("This item is not available to refund");
            }
    
            orderDetail.IndividualFashionItem.Status = FashionItemStatus.PendingForRefund;
            await _orderLineItemRepository.UpdateOrderLine(orderDetail);
            var refund = new Refund()
            {
                OrderLineItemId = refundRequest.OrderLineItemId,
                Description = refundRequest.Description,
                CreatedDate = DateTime.UtcNow,
                RefundStatus = RefundStatus.Pending,
                Images = refundRequest.Images.Select(x => new Image()
                {
                    CreatedDate = DateTime.UtcNow,
                    Url = x
                }).ToList(),
            };
            await _refundRepository.CreateRefund(refund);
            response.Data = new RefundResponse()
            {
                RefundId = refund.RefundId,
                
            };
            response.Messages = new[] { "Send refund request successfully" };
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task ChangeFashionItemsStatus(List<OrderLineItem> orderDetails,
            FashionItemStatus fashionItemStatus)
        {
            List<IndividualFashionItem> fashionItems = [];

            foreach (var orderDetail in orderDetails)
            {
                var fashionItem =
                    await _fashionItemRepository.GetFashionItemById(c =>
                        c.ItemId == orderDetail.IndividualFashionItemId!.Value);
                fashionItems.Add(fashionItem);
            }

            foreach (var fashionItem in fashionItems)
            {
                fashionItem.Status = fashionItemStatus;
                await _fashionItemRepository.UpdateFashionItem(fashionItem);
            }
        }
    }
}