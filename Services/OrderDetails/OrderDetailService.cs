using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.OrderDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.FashionItems;
using Repositories.Orders;

namespace Services.OrderDetails
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFashionItemRepository _fashionItemRepository;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository,
            IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository)
        {
            _orderDetailRepository = orderDetailRepository;
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
        }

        public async Task<Result<OrderDetailResponse<FashionItem>>> GetOrderDetailById(Guid orderId)
        {
            var response = new Result<OrderDetailResponse<FashionItem>>();
            var listOrder = await _orderDetailRepository.GetOrderDetailById(orderId);
            if (listOrder is null)
            {
                response.Messages = ["Can not found the order detail"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = listOrder;
            response.Messages = ["Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>> GetOrderDetailsByOrderId(
            Guid orderId, OrderDetailRequest request)
        {
            var response = new Result<PaginationResponse<OrderDetailResponse<FashionItem>>>();
            var listOrder = await _orderDetailRepository.GetAllOrderDetailByOrderId(orderId, request);
            if (listOrder.TotalCount == 0)
            {
                response.Messages = ["You don't have any order"];
                response.ResultStatus = ResultStatus.Empty;
                return response;
            }

            response.Data = listOrder;
            response.Messages = ["There are " + listOrder.TotalCount + " in total"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<RefundResponse>> RequestRefundToShop(Guid accountId,
            List<CreateRefundRequest> refundRequest)
        {
            var response = new Result<RefundResponse>();
            var orderDetail = await _orderDetailRepository.GetOrderDetailById(accountId);
            if (orderDetail is null)
            {
                response.Messages = ["Can not found the order to refund"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            var order = await _orderRepository.GetOrderById(orderDetail.OrderId);
            if (order.MemberId != accountId)
            {
                response.Messages = ["You are not allowed to refund this item"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            if (orderDetail.RefundExpirationDate < DateTime.UtcNow)
            {
                response.Messages = ["Expired to refund this item"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }


            var fashionitem = await _fashionItemRepository.GetFashionItemById(orderDetail.FashionItemDetail.ItemId);
            if (!fashionitem.Status.Equals(FashionItemStatus.Refundable))
            {
                response.Messages = ["This item is not allowed to refund"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = await _orderDetailRepository.CreateRefundToShop(accountId,refundRequest);
            response.Messages = new[] { "Send refund request successfully" };
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task ChangeFashionItemsStatus(List<OrderDetail> orderDetails, FashionItemStatus fashionItemStatus)
        {
            List<FashionItem> fashionItems = new List<FashionItem>();

            foreach (var orderDetail in orderDetails)
            {
                var fashionItem = await _fashionItemRepository.GetFashionItemById(orderDetail.FashionItemId!.Value);
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