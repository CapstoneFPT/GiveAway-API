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
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Utils;
using Repositories.FashionItems;
using Repositories.Orders;

namespace Services.OrderLineItems
{
    public class OrderLineItemService : IOrderLineItemService
    {
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IFashionItemRepository _fashionItemRepository;

        public OrderLineItemService(IOrderLineItemRepository orderLineItemRepository,
             IFashionItemRepository fashionItemRepository)
        {
            _orderLineItemRepository = orderLineItemRepository;
            _fashionItemRepository = fashionItemRepository;
        }

        public async Task<Result<OrderLineItemResponse<IndividualFashionItem>>> GetOrderLineItemById(Guid orderId)
        {
            var response = new Result<OrderLineItemResponse<IndividualFashionItem>>();
            var orderDetail = await _orderLineItemRepository.GetOrderLineItemById(orderId);
            if (orderDetail is null)
            {
                response.Messages = ["Can not found the order detail"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = new OrderLineItemResponse<IndividualFashionItem>()
            {
                OrderLineItemId = orderDetail.OrderLineItemId,
                OrderId = orderDetail.OrderId,
                UnitPrice = orderDetail.UnitPrice,
                RefundExpirationDate = orderDetail.RefundExpirationDate,
                FashionItemDetail = orderDetail.IndividualFashionItem
            };
            response.Messages = ["Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<PaginationResponse<OrderLineItemDetailedResponse>>> GetOrderLineItemsByOrderId(
            Guid orderId, OrderLineItemRequest request)
        {
            var response = new Result<PaginationResponse<OrderLineItemDetailedResponse>>();
            var listOrder = await _orderLineItemRepository.GetAllOrderLineItemsByOrderId(orderId, request);
            if (listOrder.TotalCount == 0)
            {
                response.Messages = ["You don't have any order"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Data = listOrder;
            response.Messages = ["There are " + listOrder.TotalCount + " in total"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<RefundResponse>> RequestRefundToShop(
            CreateRefundRequest refundRequest)
        {
            var response = new Result<RefundResponse>();
            
            var orderDetail = await _orderLineItemRepository.GetOrderLineItems(c => c.OrderLineItemId == refundRequest.OrderLineItemId);
            if (orderDetail is null)
            {
                throw new OrderNotFoundException();
            }

            
        
            if (orderDetail.Any(c => c.RefundExpirationDate < DateTime.UtcNow))
            {
                throw new RefundExpiredException("There are items that ran out refund expiration");
            }

            var fashionitemIds = orderDetail.Select(c => c.IndividualFashionItemId).ToList();
            var fashionitems = await _fashionItemRepository.GetFashionItems(c => fashionitemIds.Contains(c.ItemId));
            if (!fashionitems.Any(c => c.Status.Equals(FashionItemStatus.Refundable)))
            {
                response.Messages = ["This item is not allowed to refund"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = await _orderLineItemRepository.CreateRefundToShop(refundRequest);
            response.Messages = new[] { "Send refund request successfully" };
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task ChangeFashionItemsStatus(List<OrderLineItem> orderDetails, FashionItemStatus fashionItemStatus)
        {
            List<IndividualFashionItem> fashionItems = [];

            foreach (var orderDetail in orderDetails)
            {
                var fashionItem = await _fashionItemRepository.GetFashionItemById(c => c.ItemId == orderDetail.IndividualFashionItemId!.Value);
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