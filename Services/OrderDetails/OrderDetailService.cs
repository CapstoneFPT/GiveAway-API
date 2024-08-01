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
using BusinessObjects.Utils;
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

        public async Task<Result<PaginationResponse<OrderDetailsResponse>>> GetOrderDetailsByOrderId(
            Guid orderId, OrderDetailRequest request)
        {
            var response = new Result<PaginationResponse<OrderDetailsResponse>>();
            var listOrder = await _orderDetailRepository.GetAllOrderDetailByOrderId(orderId, request);
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
            
            var orderDetail = await _orderDetailRepository.GetOrderDetails(c => c.OrderDetailId == refundRequest.OrderDetailIds);
            if (orderDetail is null)
            {
                throw new OrderNotFoundException();
            }

            
        
            if (orderDetail.Any(c => c.RefundExpirationDate < DateTime.UtcNow))
            {
                throw new RefundExpiredException("There are items that ran out refund expiration");
            }

            var fashionitemIds = orderDetail.Select(c => c.FashionItemId).ToList();
            var fashionitems = await _fashionItemRepository.GetFashionItems(c => fashionitemIds.Contains(c.ItemId));
            if (!fashionitems.Any(c => c.Status.Equals(FashionItemStatus.Refundable)))
            {
                response.Messages = ["This item is not allowed to refund"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = await _orderDetailRepository.CreateRefundToShop(refundRequest);
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