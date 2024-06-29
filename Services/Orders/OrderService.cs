using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IMapper _mapper;
        
        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository, IMapper mapper, IOrderDetailRepository orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
            _mapper = mapper;   
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrder(List<Guid> listItemId, CreateOrderRequest orderRequest)
        {
            try
            {
                var response = new Result<OrderResponse>();
                /*int totalPrice = 0;
                Order order = new Order();
                order.MemberId = orderRequest.MemberId;
                order.PaymentMethod = orderRequest.PaymentMethod;
                order.Status = OrderStatus.AwaitingPayment;
                order.CreatedDate = DateTime.UtcNow;
                order.TotalPrice = totalPrice;
                order.OrderCode = GenerateRandomString();

                var result = await _orderRepository.CreateOrder(order);
                foreach (var id in listItemId){
                    var item = await _fashionItemRepository.GetFashionItemById(id);
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.OrderId = order.OrderId;
                    orderDetail.UnitPrice = item.SellingPrice;
                    orderDetail.FashionItemId = id;
                    await _orderDetailRepository.CreateOrderDetail(orderDetail);
                    totalPrice += item.SellingPrice;
                }
                order.TotalPrice = totalPrice;
                var resultUpdate = await _orderRepository.UpdateOrder(order);

                var data = _mapper.Map<OrderResponse>(resultUpdate);
                data.Quantity = listItemId.Count();*/


                response.Data = await _orderRepository.CreateOrderHierarchy(listItemId, orderRequest);
                response.Messages = [" Create Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<OrderResponse>>();
                var listOrder = await _orderRepository.GetOrdersByAccountId(accountId, request);
                if(listOrder.TotalCount == 0)
                {
                    response.Messages = ["You don't have any order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }
                response.Data = listOrder;
                response.Messages = ["There are " + listOrder.TotalCount + " in total"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Result<string>> CancelOrder(Guid orderId)
        {
            try
            {
                var response = new Result<string>();
                var order = await _orderRepository.GetOrderById(orderId);
                if(order == null || order.Status != OrderStatus.AwaitingPayment)
                {
                    response.Messages = ["Cound not find your order"];
                    response.ResultStatus = ResultStatus.NotFound;
                    return response;
                }
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateOrder(order);
                response.Messages = ["Your order is cancelled"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        

        
    }
}
