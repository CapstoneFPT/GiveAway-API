using System.Linq.Expressions;
using AutoMapper;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.OrderDetails;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories.Accounts;
using Repositories.AuctionItems;
using Repositories.PointPackages;
using Repositories.Shops;
using Repositories.Transactions;

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
        private readonly ITransactionRepository _transactionRepository;

        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
            IMapper mapper, IOrderDetailRepository orderDetailRepository, IAuctionItemRepository auctionItemRepository,
            IAccountRepository accountRepository, IPointPackageRepository pointPackageRepository,
            IShopRepository shopRepository, ITransactionRepository transactionRepository)
        {
            _orderRepository = orderRepository;
            _fashionItemRepository = fashionItemRepository;
            _mapper = mapper;
            _orderDetailRepository = orderDetailRepository;
            _auctionItemRepository = auctionItemRepository;
            _pointPackageRepository = pointPackageRepository;
            _accountRepository = accountRepository;
            _shopRepository = shopRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrder(Guid accountId,
            CreateOrderRequest orderRequest)
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
                orderResponse.ListItemNotAvailable = checkItemAvailable;
                response.Data = orderResponse;
                response.ResultStatus = ResultStatus.Error;
                response.Messages =
                    ["There are " + checkItemAvailable.Count + " unvailable items. Please check your order again"];
                return response;
            }

            var checkOrderExisted = await _orderRepository.IsOrderExisted(orderRequest.listItemId, accountId);
            if (checkOrderExisted.Count > 0)
            {
                var listItemExisted = checkOrderExisted.Select(x => x.FashionItemId).ToList();
                var orderResponse = new OrderResponse();
                orderResponse.ListItemNotAvailable = listItemExisted;
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

        public async Task<List<Order>> GetOrdersToCancel()
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var ordersToCancel = await _orderRepository.GetOrders(x =>
                x.CreatedDate < oneDayAgo
                && x.Status == OrderStatus.AwaitingPayment
                && x.PaymentMethod != PaymentMethod.COD);

            return ordersToCancel;
        }


        public void CancelOrders(List<Order> ordersToCancel)
        {
            ordersToCancel.ForEach(x => x.Status = OrderStatus.Cancelled);
            _orderRepository.BulkUpdate(ordersToCancel);
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
                    var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopTotal.ShopId);
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

        public async Task PayWithPoints(Guid orderId, Guid requestMemberId)
        {
            try
            {
                var order = await _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                if (order.MemberId != requestMemberId)
                {
                    throw new Exception("Not authorized");
                }

                order.Status = OrderStatus.OnDelivery;
                await _orderRepository.UpdateOrder(order);
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
                    OrderCode = _orderRepository.GenerateUniqueString(),
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
            var result = await _orderRepository.GetSingleOrder(x => x.OrderId == orderId);
            return result;
        }

        public async Task UpdateOrder(Order order)
        {
            await _orderRepository.UpdateOrder(order);
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
                orderResponse.ListItemNotAvailable = checkItemAvailable;
                response.Data = orderResponse;
                response.ResultStatus = ResultStatus.Error;
                response.Messages =
                    ["There are " + checkItemAvailable.Count + " unvailable items. Please check your order again"];
                return response;
            }

            var isitembelongshop = await _fashionItemRepository.IsItemBelongShop(shopId, orderRequest.listItemId);
            if (isitembelongshop.Count > 0)
            {
                var orderResponse = new OrderResponse();
                orderResponse.ListItemNotAvailable = isitembelongshop;
                response.Data = orderResponse;
                response.ResultStatus = ResultStatus.Error;
                response.Messages =
                [
                    "There are " + isitembelongshop.Count +
                    " items not belong to this shop. Please check your order again"
                ];
                return response;
            }

            response.Data = await _orderRepository.CreateOrderByShop(shopId, orderRequest);
            response.Messages = ["Create Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<PayOrderWithCashResponse> PayWithCash(Guid shopId, Guid orderId,
            PayOrderWithCashRequest request)
        {
            var order = await _orderRepository.GetOrderById(orderId);

            if (order!.PaymentDate != null)
            {
                throw new InvalidOperationException("Order Already Paid");
            }

            order.Status = OrderStatus.Completed;
            order.PaymentDate = DateTime.UtcNow;
            order.CompletedDate = DateTime.UtcNow;
            await _orderRepository.UpdateOrder(order);

            Expression<Func<OrderDetail, bool>> predicate = x => x.OrderId == orderId;
            Expression<Func<OrderDetail, OrderDetailsResponse>> selector = x => new OrderDetailsResponse()
            {
                OrderDetailId = x.OrderDetailId,
                ItemName = x.FashionItem!.Name,
                UnitPrice = x.UnitPrice,
                RefundExpirationDate = x.RefundExpirationDate
            };
            (List<OrderDetailsResponse> Items, int Page, int PageSize, int TotalCount) orderDetailsResponse =
                await _orderDetailRepository.GetOrderDetailsPaginate<OrderDetailsResponse>(predicate: predicate,
                    selector: selector, isTracking: false);
            var orderDetails = orderDetailsResponse.Items;

            var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopId);
            var shopAccount = await _accountRepository.GetAccountById(shop!.StaffId);
            shopAccount!.Balance += request.AmountGiven;
            await _accountRepository.UpdateAccount(shopAccount);

            var transaction = new Transaction()
            {
                OrderId = orderId,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.Purchase,
                Amount = request.AmountGiven,
            };

            await _transactionRepository.CreateTransaction(transaction);
            var response = new PayOrderWithCashResponse
            {
                AmountGiven = request.AmountGiven, OrderId = orderId,
                Order = new OrderResponse()
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    PaymentMethod = order.PaymentMethod,
                    Status = order.Status,
                    CreatedDate = order.CreatedDate,
                    TotalPrice = order.TotalPrice,
                    PaymentDate = order.PaymentDate,
                    CompletedDate = order.CompletedDate,
                    ContactNumber = order.Phone,
                    CustomerName = order.RecipientName,
                    PurchaseType = order.PurchaseType,
                    OrderDetailItems = orderDetails
                }
            };
            return response;
        }
    }
}