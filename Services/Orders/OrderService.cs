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
using BusinessObjects.Dtos.Email;
using Microsoft.Extensions.Configuration;
using Services.Emails;
using AutoMapper.Execution;
using BusinessObjects.Utils;

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
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
            IMapper mapper, IOrderDetailRepository orderDetailRepository, IAuctionItemRepository auctionItemRepository,
            IAccountRepository accountRepository, IPointPackageRepository pointPackageRepository,
            IShopRepository shopRepository, ITransactionRepository transactionRepository,
            IConfiguration configuration, IEmailService emailService)
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
            _configuration = configuration;
            _emailService = emailService;
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
                        ["This order of your shop is finally delivered! The order status has changed to completed"];
                }
                else
                {
                    response.Messages =
                        ["The order of your shop is delivered! The item status has changed to refundable"];
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
                    ["There are " + checkItemAvailable.Count + " unavailable items. Please check your order again"];
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

            if (request.AmountGiven < order.TotalPrice)
            {
                throw new InvalidOperationException("Not enough money");
            }

            if (order.PaymentMethod != PaymentMethod.Cash)
            {
                throw new InvalidOperationException("This order can only be paid with cash");
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
            await SendEmailOrder(order);
            var response = new PayOrderWithCashResponse
            {
                AmountGiven = request.AmountGiven, OrderId = orderId,
                Order = new OrderResponse()
                {
                    OrderId = order.OrderId,
                    Quantity = orderDetails.Count,
                    OrderCode = order.OrderCode,
                    PaymentMethod = order.PaymentMethod,
                    Status = order.Status,
                    CreatedDate = order.CreatedDate,
                    Address = order.Address,
                    TotalPrice = order.TotalPrice,
                    PaymentDate = order.PaymentDate,
                    CompletedDate = order.CompletedDate,
                    ContactNumber = order.Phone,
                    RecipientName = order.RecipientName,
                    PurchaseType = order.PurchaseType,
                    OrderDetailItems = orderDetails
                }
            };
            return response;
        }

        public async Task<Result<string>> SendEmailOrder(Order order)
        {
            var response = new Result<string>();
            if (order.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(order.MemberId.Value);

                SendEmailRequest content = new SendEmailRequest
                {
                    To = member.Email,
                    Subject = $"[GIVEAWAY] Invoice from GiveAway {order.OrderCode}",
                    Body = $@"<h3>Dear customer,<h3>
                        <h5>Thank you for purchase at GiveAway<h5><br>
                        <h5>Here is your invoice detail<h5>
                        <p>Order Code: {order.OrderCode}<p>
                        <p>Total Price: {order.TotalPrice}<p>
                        <p>Purchase Date: {order.CreatedDate}<p>
                        <p>Payment Method: {order.PaymentMethod}<p>
                        <p>Payment Date: {order.PaymentDate}<p>
                        
                    ",
                };
                await _emailService.SendEmail(content);
                response.Messages = ["The invoice has been send to customer mail"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            else
            {
                SendEmailRequest content = new SendEmailRequest
                {
                    To = order.Email,
                    Subject = $"[GIVEAWAY] Invoice from GiveAway {order.OrderCode}",
                    Body = $@"<h3>Dear customer,<h3><br>
                        <h4>Thank you for your buying at GiveAway<h4><br>
                        <h4>Here is your invoice detail<h4><br>
                        
                        
                    ",
                };
                await _emailService.SendEmail(content);
                response.Messages = ["The invoice has been send to customer mail"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
        }

        public async Task UpdateAdminBalance(Order order)
        {
            //This is the admin account, we will have only ONE admin account
            var account = await _accountRepository.GetAccountById(new Guid("a8a95941-cb06-6967-5eb5-26cd1f562b6b"));

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            account!.Balance += order.TotalPrice;
            await _accountRepository.UpdateAccount(account);
        }
    }
}