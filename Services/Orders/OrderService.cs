using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using BusinessObjects.Dtos.Auctions;
using Repositories.Accounts;
using Repositories.AuctionItems;
using Repositories.PointPackages;
using Repositories.Shops;

namespace Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IAuctionItemRepository _auctionItemRepository;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        private readonly IPointPackageRepository _pointPackageRepository;
        private readonly IShopRepository _shopRepository;

        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
            IMapper mapper, IOrderDetailRepository orderDetailRepository, IAuctionItemRepository auctionItemRepository,
            IAccountRepository accountRepository, IPointPackageRepository pointPackageRepository,
            IShopRepository shopRepository)
        {
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
            _mapper = mapper;
            _orderDetailRepository = orderDetailRepository;
            _auctionItemRepository = auctionItemRepository;
            _pointPackageRepository = pointPackageRepository;
            _accountRepository = accountRepository;
            _shopRepository = shopRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrder(Guid accountId, 
            CreateOrderRequest orderRequest)
        {
            try
            {
                var response = new Result<OrderResponse>();
                if (orderRequest.listItemId.Count == 0)
                {
                    response.Messages = ["You have no item for order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }

                var checkItemAvailable = await _orderRepository.IsOrderAvailable(orderRequest.listItemId);
                if (checkItemAvailable.Count > 0)
                {
                    var orderResponse = new OrderResponse();
                    orderResponse.listItemExisted = checkItemAvailable;
                    response.Data = orderResponse;
                    response.ResultStatus = ResultStatus.Error;
                    response.Messages = ["There are some unvailable items. Please check your order again"];
                    return response;
                }

                var checkOrderExisted = await _orderRepository.IsOrderExisted(orderRequest.listItemId, accountId);
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

                response.Data = await _orderRepository.CreateOrderHierarchy(accountId, orderRequest);
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
                var orderResult = await _orderRepository.CreateOrder(toBeAdded);

                var orderDetails =
                        new OrderDetail()
                        {
                            OrderId = orderResult.OrderId,
                            FashionItemId = orderRequest.AuctionFashionItemId,
                            UnitPrice = orderRequest.TotalPrice,
                        }
                    ;
                var orderDetailResult =
                    await _orderDetailRepository.CreateOrderDetail(orderDetails);

                orderResult.OrderDetails = new List<OrderDetail>() { orderDetailResult };
                return new Result<OrderResponse>()
                {
                    Data = _mapper.Map<Order, OrderResponse>(orderResult),
                    ResultStatus = ResultStatus.Success
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<OrderDetail>> GetOrderDetailByOrderId(Guid orderId)
        {
            try
            {
                return await _orderDetailRepository.GetOrderDetails(x => x.OrderId == orderId);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<Order>> GetOrdersToCancel()
        {
            try
            {
                var oneDayAgo = DateTime.UtcNow.AddDays(-1);
                var ordersToCancel = await _orderRepository.GetOrders(x =>
                    x.CreatedDate < oneDayAgo
                    && x.Status == OrderStatus.AwaitingPayment
                    && x.PaymentMethod != PaymentMethod.COD);

                return ordersToCancel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        

        public void CancelOrders(List<Order> ordersToCancel)
        {
            try
            {
                ordersToCancel.ForEach(x => x.Status = OrderStatus.Cancelled);
                _orderRepository.BulkUpdate(ordersToCancel);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateShopBalance(Order order)
        {
            try
            {
                if (order.Status != OrderStatus.Completed)
                {
                    throw new Exception("Can not update balance if order is not completed");
                }

                var shopTotals = order.OrderDetails
                    .GroupBy(item => item.FashionItem!.ShopId)
                    .Select(group =>
                        new
                        {
                            ShopId = group.Key,
                            Total = group.Sum(item => item.UnitPrice)
                        });

                foreach (var shopTotal in shopTotals)
                {
                    var shop = await _shopRepository.GetSingleShop(x=>x.ShopId == shopTotal.ShopId);
                    var staff = await _accountRepository.GetAccountById(shop!.StaffId);
                    staff.Balance += shopTotal.Total;
                    await _accountRepository.UpdateAccount(staff);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateFashionItemStatus(Guid orderOrderId)
        {
            try
            {
                var orderDetails = await _orderDetailRepository.GetOrderDetails(x => x.OrderId == orderOrderId);
                orderDetails.ForEach(x => x.FashionItem!.Status = FashionItemStatus.Unavailable);
                var fashionItems = orderDetails.Select(x => x.FashionItem).ToList();
                await _fashionItemRepository.BulkUpdate(fashionItems!);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async void CancelOrder(Order x)
        {
            x.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateOrder(x);
        }

        public async Task<Result<OrderResponse>> CreatePointPackageOrder(PointPackageOrder order)
        {
            try
            {
                var orderResult = await _orderRepository.CreateOrder(new Order()
                {
                    OrderCode = OrderRepository.GenerateUniqueString(),
                    CreatedDate = DateTime.UtcNow,
                    MemberId = order.MemberId,
                    TotalPrice = order.TotalPrice,
                    PaymentMethod = order.PaymentMethod,
                    Status = OrderStatus.AwaitingPayment,
                });

                var orderDetailResult = await _orderDetailRepository.CreateOrderDetail(new OrderDetail()
                {
                    OrderId = orderResult.OrderId,
                    UnitPrice = order.TotalPrice,
                    PointPackageId = order.PointPackageId,
                });

                return new Result<OrderResponse>()
                {
                    Data = _mapper.Map<Order, OrderResponse>(orderResult),
                    ResultStatus = ResultStatus.Success
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task<Order?> GetOrderById(Guid orderId)
        {
            try
            {
                var result = await _orderRepository.GetSingleOrder(x => x.OrderId == orderId);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateOrder(Order order)
        {
            try
            {
                await _orderRepository.UpdateOrder(order);
            }
            catch (Exception e)
            {
                throw new Exception();
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

        public async Task<Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId, Guid orderId)
        {
            try
            {
                var response = new Result<OrderResponse>();
                var order = await _orderRepository.GetOrderById(orderId);
                if (order == null || order.Status != OrderStatus.OnDelivery)
                {
                    response.Messages = ["Cound not find your order"];
                    response.ResultStatus = ResultStatus.NotFound;
                    return response;
                }

                var orderResponse = await _orderRepository.ConfirmOrderDelivered(shopId, orderId);
                response.Data = orderResponse;
                if (orderResponse.Status.Equals(OrderStatus.Completed))
                {
                    response.Messages =
                        ["This order of your shop is finally deliveried! The order status has changed to completed"];
                }
                else
                {
                    response.Messages =
                        ["The order of your shop is deliveried! The item status has changed to refundable"];
                }

                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<OrderResponse>> CreateOrderByShop(Guid shopId, CreateOrderRequest orderRequest)
        {
            try
            {
                var response = new Result<OrderResponse>();
                if (orderRequest.listItemId.Count == 0)
                {
                    response.Messages = ["You have no item for order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }

                var checkItemAvailable = await _orderRepository.IsOrderAvailable(orderRequest.listItemId);
                if (checkItemAvailable.Count > 0)
                {
                    var orderResponse = new OrderResponse();
                    orderResponse.listItemExisted = checkItemAvailable;
                    response.Data = orderResponse;
                    response.ResultStatus = ResultStatus.Error;
                    response.Messages = ["There are some unvailable items. Please check your order again"];
                    return response;
                }

                response.Data = await _orderRepository.CreateOrderByShop(shopId, orderRequest);
                response.Messages = ["Create Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}