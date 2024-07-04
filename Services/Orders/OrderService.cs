using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using MailKit.Search;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Repositories.AuctionItems;

namespace Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IAuctionItemRepository _auctionItemRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
            IMapper mapper, IOrderDetailRepository orderDetailRepository, IAuctionItemRepository auctionItemRepository)
        {
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
            _mapper = mapper;
            _orderDetailRepository = orderDetailRepository;
            _auctionItemRepository = auctionItemRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrder(Guid accountId ,List<Guid?> listItemId, CreateOrderRequest orderRequest)
        {
            try
            {
                var response = new Result<OrderResponse>();
                if (listItemId.Count == 0)
                {
                    response.Messages = ["You have no item for order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }

                var checkItemAvailable = await _orderRepository.IsOrderAvailable(listItemId);
                if (checkItemAvailable.Count > 0)
                {
                    var orderResponse = new OrderResponse();
                    orderResponse.listItemExisted = checkItemAvailable;
                    response.Data = orderResponse;
                    response.ResultStatus = ResultStatus.Error;
                    response.Messages = ["There are some unvailable items. Please check your order again"];
                    return response;
                }

                var checkOrderExisted = await _orderRepository.IsOrderExisted(listItemId, accountId);
                if (checkOrderExisted.Count > 0)
                {
                    var listItemExisted = checkOrderExisted.Select(x => x.FashionItemId).ToList();
                    var orderResponse = new OrderResponse();
                    orderResponse.listItemExisted = listItemExisted;
                    response.Data = orderResponse;
                    response.ResultStatus = ResultStatus.Duplicated;
                    response.Messages = ["You already order those items. Please remove them"];
                    return response;
                }

                response.Data = await _orderRepository.CreateOrderHierarchy(accountId,listItemId, orderRequest);
                response.Messages = ["Create Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<Result<OrderResponse>> CreateOrderFromBid(CreateOrderFromBidRequest orderRequest)
        {
            try
            {
                var toBeAdded = new Order()
                {
                    BidId = orderRequest.BidId,
                    OrderCode = orderRequest.OrderCode,
                    PaymentMethod = orderRequest.PaymentMethod,
                    MemberId = orderRequest.MemberId,
                    TotalPrice = orderRequest.TotalPrice,
                    CreatedDate = DateTime.UtcNow,
                };

                var orderDetail = new OrderDetail()
                {
                    OrderId = toBeAdded.OrderId,
                    FashionItemId = orderRequest.AuctionFashionItemId,
                    UnitPrice = orderRequest.TotalPrice,
                };

                toBeAdded.OrderDetails.Add(orderDetail);

                var result = await _orderRepository.CreateOrder(toBeAdded);


                return new Result<OrderResponse>()
                {
                    Data = _mapper.Map<Order, OrderResponse>(result),
                    ResultStatus = ResultStatus.Success
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId,
            OrderRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<OrderResponse>>();
                var listOrder = await _orderRepository.GetOrdersByAccountId(accountId, request);
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
            catch (Exception ex)
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
                if (order == null || order.Status != OrderStatus.AwaitingPayment)
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByShopId(Guid shopId,
            OrderRequest orderRequest)
        {
            try
            {
                var response = new Result<PaginationResponse<OrderResponse>>();
                var order = await _orderRepository.GetOrdersByShopId(shopId, orderRequest);
                if (order.TotalCount == 0)
                {
                    response.Messages = ["Cound not find your order"];
                    response.ResultStatus = ResultStatus.NotFound;
                    return response;
                }

                response.Data = order;
                response.Messages = ["Your list contains " + order.TotalCount + " orders"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<string>> ConfirmOrderDeliveried(Guid orderId)
        {
            try
            {
                var response = new Result<string>();
                var order = await _orderRepository.GetOrderById(orderId);
                if (order == null || order.Status != OrderStatus.AwaitingPayment)
                {
                    response.Messages = ["Cound not find your order"];
                    response.ResultStatus = ResultStatus.NotFound;
                    return response;
                }
                order.Status = OrderStatus.Completed;
                await _orderRepository.UpdateOrder(order);
                response.Messages = ["This order is completed! The status has changed to completed"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class CreateOrderFromBidRequest
    {
        public int TotalPrice { get; set; }
        public string OrderCode { get; set; }
        public Guid BidId { get; set; }
        public Guid MemberId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid AuctionFashionItemId { get; set; }
    }
}