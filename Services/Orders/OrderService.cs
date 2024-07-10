using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Utils;
using Repositories.Accounts;
using Repositories.Shops;
using Services.Transactions;

namespace Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;

        private readonly IShopRepository _shopRepository;

        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
            IMapper mapper, IOrderDetailRepository orderDetailRepository,
            IAccountRepository accountRepository,
            IShopRepository shopRepository)
        {
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
            _mapper = mapper;
            _orderDetailRepository = orderDetailRepository;

            _accountRepository = accountRepository;
            _shopRepository = shopRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrder(Guid accountId,
            CreateOrderRequest order)
        {
            var response = new Result<OrderResponse>();
            if (order.listItemId.Count == 0)
            {
                response.Messages = ["You have no item for order"];
                response.ResultStatus = ResultStatus.Empty;
                return response;
            }

            var checkItemAvailable = await _orderRepository.IsOrderAvailable(order.listItemId);
            if (checkItemAvailable.Count > 0)
            {
                var orderResponse = new OrderResponse();
                orderResponse.listItemExisted = checkItemAvailable;
                response.Data = orderResponse;
                response.ResultStatus = ResultStatus.Error;
                response.Messages = ["There are some unvailable items. Please check your order again"];
                return response;
            }

            var checkOrderExisted = await _orderRepository.IsOrderExisted(order.listItemId, accountId);
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

            response.Data = await _orderRepository.CreateOrderHierarchy(accountId, order);
            response.Messages = ["Create Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<OrderResponse>> CreateOrderFromBid(CreateOrderFromBidRequest orderRequest)
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

        public async Task<List<OrderDetail>> GetOrderDetailByOrderId(Guid orderId)
        {
            return await _orderDetailRepository.GetOrderDetails(x => x.OrderId == orderId);
        }

        public async Task<List<Order?>> GetOrdersToCancel()
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var ordersToCancel = await _orderRepository.GetOrders(x =>
                x.CreatedDate < oneDayAgo
                && x.Status == OrderStatus.AwaitingPayment
                && x.PaymentMethod != PaymentMethod.COD);

            return ordersToCancel;
        }


        public void CancelOrders(List<Order?> ordersToCancel)
        {
            ordersToCancel.ForEach(x => x.Status = OrderStatus.Cancelled);
            _orderRepository.BulkUpdate(ordersToCancel);
        }

        public async Task UpdateShopBalance(Order order)
        {
            if (order.Status != OrderStatus.Completed)
            {
                throw new CannotUpdateShopBalanceException("Order status is not completed");
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
                var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopTotal.ShopId);
                var staff = await _accountRepository.GetAccountById(shop!.StaffId);
                staff.Balance += shopTotal.Total;
                await _accountRepository.UpdateAccount(staff);
            }
        }

        public async Task UpdateFashionItemStatus(Guid orderOrderId)
        {
            var orderDetails = await _orderDetailRepository.GetOrderDetails(x => x.OrderId == orderOrderId);
            orderDetails.ForEach(x => x.FashionItem!.Status = FashionItemStatus.Unavailable);
            var fashionItems = orderDetails.Select(x => x.FashionItem).ToList();
            await _fashionItemRepository.BulkUpdate(fashionItems!);
        }

        public async Task PayWithPoints(Guid orderId, Guid requestMemberId)
        {
            var order = await _orderRepository.GetOrderById(orderId);

            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            if (order.MemberId != requestMemberId)
            {
                throw new NotAuthorizedToPayOrderException();
            }

            order.Status = OrderStatus.OnDelivery;
            await _orderRepository.UpdateOrder(order);
        }

        private async Task CancelOrder(Order? x)
        {
            x.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateOrder(x);
        }

        public async Task<Result<OrderResponse>> CreatePointPackageOrder(PointPackageOrder order)
        {
            var orderResult = await _orderRepository.CreateOrder(new Order()
            {
                OrderCode = _orderRepository.GenerateUniqueString(),
                CreatedDate = DateTime.UtcNow,
                MemberId = order.MemberId,
                TotalPrice = order.TotalPrice,
                PaymentMethod = order.PaymentMethod,
                Status = OrderStatus.AwaitingPayment,
            });

            await _orderDetailRepository.CreateOrderDetail(new OrderDetail()
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


        public async Task<Order?> GetOrderById(Guid orderId)
        {
            var result = await _orderRepository.GetSingleOrder(x => x.OrderId == orderId);
            return result;
        }

        public async Task UpdateOrder(Order? order)
        {
            await _orderRepository.UpdateOrder(order);
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId,
            OrderRequest request)
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

        public async Task<Result<string>> CancelOrder(Guid orderId)
        {
            var response = new Result<string>();
            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.AwaitingPayment)
            {
                response.Messages = ["Could not find your order"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateOrder(order);
            response.Messages = ["Your order is cancelled"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByShopId(Guid shopId,
            OrderRequest orderRequest)
        {
            var response = new Result<PaginationResponse<OrderResponse>>();
            var order = await _orderRepository.GetOrdersByShopId(shopId, orderRequest);
            if (order.TotalCount == 0)
            {
                response.Messages = ["Could not find your order"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = order;
            response.Messages = ["Your list contains " + order.TotalCount + " orders"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId, Guid orderId)
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
                    ["This order of your shop is finally delivered! The order status has changed to completed"];
            }
            else
            {
                response.Messages =
                    ["The order of your shop is deliveried! The item status has changed to refundable"];
            }

            response.ResultStatus = ResultStatus.Success;
            return response;
        }


        public async Task<Result<OrderResponse>> CreateOrderByShop(Guid shopId, CreateOrderRequest request)
        {
            var response = new Result<OrderResponse>();
            if (request.listItemId.Count == 0)
            {
                response.Messages = ["You have no item for order"];
                response.ResultStatus = ResultStatus.Empty;
                return response;
            }

            var checkItemAvailable = await _orderRepository.IsOrderAvailable(request.listItemId);
            if (checkItemAvailable.Count > 0)
            {
                var orderResponse = new OrderResponse();
                orderResponse.listItemExisted = checkItemAvailable;
                response.Data = orderResponse;
                response.ResultStatus = ResultStatus.Error;
                response.Messages = ["There are some unvailable items. Please check your order again"];
                return response;
            }

            response.Data = await _orderRepository.CreateOrderByShop(shopId, request);
            response.Messages = ["Create Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }
    }
}