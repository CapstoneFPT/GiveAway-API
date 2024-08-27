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
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Repositories.Refunds;
using Services.ConsignSales;
using Services.FashionItems;
using Services.GiaoHangNhanh;

namespace Services.Orders;

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
    private readonly IRefundRepository _refundRepository;
    private readonly IGiaoHangNhanhService _giaoHangNhanhService;
    private readonly ILogger<OrderService> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
        IMapper mapper, IOrderDetailRepository orderDetailRepository, IAuctionItemRepository auctionItemRepository,
        IAccountRepository accountRepository, IPointPackageRepository pointPackageRepository,
        IShopRepository shopRepository, ITransactionRepository transactionRepository,
        IConfiguration configuration, IEmailService emailService, IRefundRepository refundRepository,
        IGiaoHangNhanhService giaoHangNhanhService, ILogger<OrderService> logger, ISchedulerFactory schedulerFactory)
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
        _refundRepository = refundRepository;
        _giaoHangNhanhService = giaoHangNhanhService;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrder(Guid accountId,
        CartRequest cart)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();
        if (cart.PaymentMethod.Equals(PaymentMethod.Cash))
        {
            throw new WrongPaymentMethodException("Not allow to pay with cash");
        }

        if (cart.CartItems.Count == 0)
        {
            response.Messages = ["You have no item for order"];
            response.ResultStatus = ResultStatus.Error;
            return response;
        }

        var checkItemAvailable = await _orderRepository.IsOrderAvailable(cart.CartItems.Select(ci => ci.ItemId as Guid?).ToList());
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

        var checkOrderExisted = await _orderRepository.IsOrderExisted(cart.CartItems.Select(ci => ci.ItemId as Guid?).ToList(), accountId);
        if (checkOrderExisted.Count > 0)
        {
            var listItemExisted = checkOrderExisted.Select(x => x.IndividualFashionItemId).ToList();
            var orderResponse = new OrderResponse();
            orderResponse.ListItemNotAvailable = listItemExisted;
            response.Data = orderResponse;
            response.ResultStatus = ResultStatus.Duplicated;
            response.Messages = ["You already order those items. Please remove them"];
            return response;
        }

        response.Data = await _orderRepository.CreateOrderHierarchy(accountId, cart);
        response.Messages = ["Create Successfully"];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrderFromBid(
        CreateOrderFromBidRequest orderRequest)
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
                    IndividualFashionItemId = orderRequest.AuctionFashionItemId,
                    UnitPrice = orderRequest.TotalPrice,
                    CreatedDate = DateTime.UtcNow,
                }
            ;
        var orderDetailResult =
            await _orderDetailRepository.CreateOrderDetail(orderDetails);

        orderResult.OrderDetails = new List<OrderDetail>() { orderDetailResult };
        return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
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


    public async Task CancelOrders(List<Order?> ordersToCancel)
    {
        foreach (var order in ordersToCancel)
        {
            order!.Status = OrderStatus.Cancelled;
        }

        await _orderRepository.BulkUpdate(ordersToCancel!);
    }

    public async Task UpdateShopBalance(Order order)
    {
        if (order.Status != OrderStatus.Completed)
        {
            throw new Exception("Can not update balance if order is not completed");
        }

        // var shopTotals = order.OrderDetails
        //     .GroupBy(item => item.IndividualFashionItem.ShopId)
        //     .Select(group =>
        //         new
        //         {
        //             ShopId = group.Key,
        //             Total = group.Sum(item => item.UnitPrice)
        //         });
        //
        // foreach (var shopTotal in shopTotals)
        // {
        //     var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopTotal.ShopId);
        //     var staff = await _accountRepository.GetAccountById(shop!.StaffId);
        //     staff.Balance += shopTotal.Total;
        //     await _accountRepository.UpdateAccount(staff);
        // }
    }

    public async Task UpdateFashionItemStatus(Guid orderOrderId)
    {
        var orderDetails = await _orderDetailRepository.GetOrderDetails(x => x.OrderId == orderOrderId);
        orderDetails.ForEach(x => x.IndividualFashionItem.Status = FashionItemStatus.PendingForOrder);
        var fashionItems = orderDetails.Select(x => x.IndividualFashionItem).ToList();
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


    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreatePointPackageOrder(
        PointPackageOrder order)
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

        return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
        {
            Data = new OrderResponse()
            {
                OrderId = orderResult.OrderId,
                OrderCode = orderResult.OrderCode,
                TotalPrice = orderResult.TotalPrice,
                OrderDetailItems = new List<OrderDetailsResponse>()
                {
                    new OrderDetailsResponse()
                    {
                        OrderDetailId = orderDetailResult.OrderDetailId,
                        UnitPrice = orderDetailResult.UnitPrice,
                        RefundExpirationDate = null,
                        PointPackageId = orderDetailResult.PointPackageId
                    }
                },
                CreatedDate = orderResult.CreatedDate,
                // PaymentDate = orderResult.PaymentDate,
            },
            ResultStatus = ResultStatus.Success
        };
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

    public async Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<OrderListResponse>>> GetOrdersByAccountId(
        Guid accountId,
        OrderRequest request)
    {
        

        Expression<Func<Order, bool>> predicate = order => order.MemberId == accountId;
        Expression<Func<Order, OrderListResponse>> selector = order => new OrderListResponse()
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedDate = order.CreatedDate,
            // PaymentDate = order.PaymentDate,
            MemberId = order.MemberId,
            CompletedDate = order.CompletedDate,
            ContactNumber = order.Phone,
            RecipientName = order.RecipientName,
            PurchaseType = order.PurchaseType,
            Address = order.Address,
            PaymentMethod = order.PaymentMethod,
            CustomerName = order.Member.Fullname,
            Email = order.Email,
            Quantity = order.OrderDetails.Count,
            AuctionTitle = order.Bid.Auction.Title,
            ShippingFee = order.ShippingFee,
            Discount = order.Discount
        };

        if (request.Status != null)
        {
            predicate = order => order.Status == request.Status;
        }

        if (!string.IsNullOrEmpty(request.OrderCode))
        {
            predicate = predicate.And(order => EF.Functions.ILike(order.OrderCode, $"%{request.OrderCode}%"));
        }

        if (request.ShopId.HasValue)
        {
            predicate = predicate.And(order =>
                order.OrderDetails.Any(c => c.IndividualFashionItem.Variation!.MasterItem.ShopId == request.ShopId.Value));
        }

        if (request.PaymentMethod != null)
        {
            predicate = predicate.And(order => order.PaymentMethod == request.PaymentMethod);
        }

        if (request.IsFromAuction == true)
        {
            predicate = predicate.And(ord => ord.BidId != null);
        }

        if (request.IsFromAuction == false)
        {
            predicate = predicate.And(ord => ord.BidId == null);
        }

        if (request.IsPointPackage == true)
        {
            predicate = predicate.And(ord => ord.BidId != null);
        }

        if (request.IsFromAuction == true)
        {
            predicate = predicate.And(ord => ord.BidId != null);
        }

        (List<OrderListResponse> Items, int Page, int PageSize, int TotalCount) =
            await _orderRepository.GetOrdersProjection<OrderListResponse>(request.PageNumber,
                request.PageSize, predicate, selector);

        return new BusinessObjects.Dtos.Commons.Result<PaginationResponse<OrderListResponse>>()
        {
            Data = new PaginationResponse<OrderListResponse>()
            {
                Items = Items,
                PageNumber = Page,
                PageSize = PageSize,
                TotalCount = TotalCount,
                SearchTerm = request.OrderCode
            },
            ResultStatus = ResultStatus.Success
        };
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<string>> CancelOrder(Guid orderId)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<string>();
        var order = await _orderRepository.GetSingleOrder(c => c.OrderId == orderId);
        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (!order.Status.Equals(OrderStatus.Pending) && !order.Status.Equals(OrderStatus.AwaitingPayment))
        {
            throw new StatusNotAvailableException();
        }

        if (order.Status.Equals(OrderStatus.Pending) && !order.PaymentMethod.Equals(PaymentMethod.COD))
        {
            order.Member.Balance += order.TotalPrice;
            var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
            if (admin == null)
                throw new AccountNotFoundException();
            admin.Balance -= order.TotalPrice;
            await _accountRepository.UpdateAccount(admin);
            var transaction = new Transaction()
            {
                OrderId = orderId,
                MemberId = order.MemberId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.Refund
            };
            await _transactionRepository.CreateTransaction(transaction);
        }

        order.Status = OrderStatus.Cancelled;
        foreach (var item in order.OrderDetails.Select(c => c.IndividualFashionItem))
        {
            item.Status = FashionItemStatus.Available;
        }

        await _orderRepository.UpdateOrder(order);
        response.Messages = ["Your order is cancelled"];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<string>> CancelOrderByAdmin(Guid orderId)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<string>();
        var order = await _orderRepository.GetSingleOrder(c => c.OrderId == orderId);
        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (order.Status.Equals(OrderStatus.Completed))
        {
            throw new StatusNotAvailableException();
        }

        if ((order.Status.Equals(OrderStatus.OnDelivery) || order.Status.Equals(OrderStatus.Pending))
            && !order.PaymentMethod.Equals(PaymentMethod.COD))
        {
            order.Member.Balance += order.TotalPrice;
            var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
            if (admin == null)
                throw new AccountNotFoundException();
            admin.Balance -= order.TotalPrice;
            await _accountRepository.UpdateAccount(admin);
            var transaction = new Transaction()
            {
                OrderId = orderId,
                MemberId = order.MemberId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.Refund
            };
            await _transactionRepository.CreateTransaction(transaction);
        }
        else
        {
            throw new StatusNotAvailableException();
        }

        order.Status = OrderStatus.Cancelled;
        foreach (var item in order.OrderDetails.Select(c => c.IndividualFashionItem))
        {
            item.Status = FashionItemStatus.Unavailable;
        }

        await _orderRepository.UpdateOrder(order);
        /*await _emailService.SendEmailCancelOrderByShop(order);*/
        response.Messages = ["This order is cancelled by shop for some reason."];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }


    public async Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<OrderListResponse>>> GetOrders(
        OrderRequest orderRequest)
    {
        Expression<Func<Order, bool>> predicate = order => true;
        Expression<Func<Order, OrderListResponse>> selector = order => new OrderListResponse()
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedDate = order.CreatedDate,
            // PaymentDate = order.PaymentDate,
            MemberId = order.MemberId,
            CompletedDate = order.CompletedDate,
            ContactNumber = order.Phone,
            RecipientName = order.RecipientName,
            PurchaseType = order.PurchaseType,
            Address = order.Address,
            PaymentMethod = order.PaymentMethod,
            CustomerName = order.Member.Fullname,
            Email = order.Email,
            Quantity = order.OrderDetails.Count,
            AuctionTitle = order.Bid.Auction.Title
        };

        if (orderRequest.Status != null)
        {
            predicate = order => order.Status == orderRequest.Status;
        }

        if (!string.IsNullOrEmpty(orderRequest.OrderCode))
        {
            predicate = predicate.And(order => EF.Functions.ILike(order.OrderCode, $"%{orderRequest.OrderCode}%"));
        }

        if (orderRequest.ShopId.HasValue)
        {
            // predicate = predicate.And(order =>
            //     order.OrderDetails.Any(c => c.IndividualFashionItem.ShopId == orderRequest.ShopId.Value));
        }

        if (orderRequest.PaymentMethod != null)
        {
            predicate = predicate.And(order => order.PaymentMethod == orderRequest.PaymentMethod);
        }

        if (orderRequest.IsFromAuction == true)
        {
            predicate = predicate.And(ord => ord.BidId != null);
        }

        if (orderRequest.IsFromAuction == false)
        {
            predicate = predicate.And(ord => ord.BidId == null);
        }

        if (orderRequest.IsPointPackage == true)
        {
            predicate = predicate.And(or => or.OrderDetails.All(c => c.PointPackageId != null));
        }

        if (orderRequest.IsPointPackage == false)
        {
            predicate = predicate.And(or => or.OrderDetails.All(c => c.PointPackageId == null));
        }

        (List<OrderListResponse> Items, int Page, int PageSize, int TotalCount) =
            await _orderRepository.GetOrdersProjection<OrderListResponse>(orderRequest.PageNumber,
                orderRequest.PageSize, predicate, selector);

        return new BusinessObjects.Dtos.Commons.Result<PaginationResponse<OrderListResponse>>()
        {
            Data = new PaginationResponse<OrderListResponse>()
            {
                Items = Items,
                PageNumber = Page,
                PageSize = PageSize,
                TotalCount = TotalCount,
                SearchTerm = orderRequest.OrderCode
            },
            ResultStatus = ResultStatus.Success
        };
    }


    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId ,Guid orderId)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null || order.Status != OrderStatus.OnDelivery)
        {
            throw new OrderNotFoundException();
        }

        var orderResponse = await _orderRepository.ConfirmOrderDelivered(shopId , orderId);
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

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrderByShop(Guid shopId,
        CreateOrderRequest orderRequest)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();
        if (orderRequest.ItemIds.Count == 0)
        {
            response.Messages = ["You have no item for order"];
            response.ResultStatus = ResultStatus.Error;
            return response;
        }


        var checkItemAvailable = await _orderRepository.IsOrderAvailable(orderRequest.ItemIds);
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

        var isitembelongshop = await _fashionItemRepository.IsItemBelongShop(shopId, orderRequest.ItemIds);
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

        if (order!.OrderDetails.All(c => c.PaymentDate != null))
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
        // order.PaymentDate = DateTime.UtcNow;
        order.CompletedDate = DateTime.UtcNow;
        await _orderRepository.UpdateOrder(order);

        var listorderDetail = await _orderDetailRepository.GetOrderDetails(c => c.OrderId == orderId);
        foreach (var itemOrderDetail in listorderDetail)
        {
            itemOrderDetail.RefundExpirationDate = DateTime.UtcNow;
            itemOrderDetail.IndividualFashionItem.Status = FashionItemStatus.Refundable;
        }

        await _orderDetailRepository.UpdateRange(listorderDetail);
        Expression<Func<OrderDetail, bool>> predicate = x => x.OrderId == orderId;
        Expression<Func<OrderDetail, OrderDetailsResponse>> selector = x => new OrderDetailsResponse()
        {
            OrderDetailId = x.OrderDetailId,
            ItemName = x.IndividualFashionItem.Variation.MasterItem.Name,
            UnitPrice = x.UnitPrice,
            RefundExpirationDate = x.RefundExpirationDate,
            PaymentDate = x.PaymentDate
        };
        (List<OrderDetailsResponse> Items, int Page, int PageSize, int TotalCount) orderDetailsResponse =
            await _orderDetailRepository.GetOrderDetailsPaginate<OrderDetailsResponse>(predicate: predicate,
                selector: selector, isTracking: false);
        var orderDetails = orderDetailsResponse.Items;

        var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopId);
        var shopAccount = await _accountRepository.GetAccountById(shop!.StaffId);
        shopAccount!.Balance += order.TotalPrice;
        await _accountRepository.UpdateAccount(shopAccount);

        var transaction = new Transaction()
        {
            OrderId = orderId,
            CreatedDate = DateTime.UtcNow,
            Type = TransactionType.Purchase,
            Amount = order.TotalPrice,
        };

        await _transactionRepository.CreateTransaction(transaction);

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
                // PaymentDate = order.PaymentDate,
                CompletedDate = order.CompletedDate,
                ContactNumber = order.Phone,
                RecipientName = order.RecipientName,
                PurchaseType = order.PurchaseType,
                OrderDetailItems = orderDetails
            }
        };
        return response;
    }


    public async Task UpdateAdminBalance(Order order)
    {
        //This is the admin account, we will have only ONE admin account
        var account = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));

        if (account == null)
        {
            throw new AccountNotFoundException();
        }

        account!.Balance += order.TotalPrice;
        await _accountRepository.UpdateAccount(account);
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmPendingOrder(Guid orderdetailId, FashionItemStatus itemStatus)
    {
        var order = await _orderRepository.GetSingleOrder(c => c.OrderDetails.Any(c => c.OrderDetailId == orderdetailId));
        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (!order.Status.Equals(OrderStatus.Pending))
        {
            throw new StatusNotAvailableException();
        }
        if(!itemStatus.Equals(FashionItemStatus.OnDelivery) && !itemStatus.Equals(FashionItemStatus.Unavailable))
        {
            throw new StatusNotAvailableException();
        }

        var orderDetail = order.OrderDetails.FirstOrDefault(c => c.OrderDetailId == orderdetailId);
        
        
        if (orderDetail == null)
        {
            throw new OrderDetailNotFoundException();
        }

        if (!orderDetail.IndividualFashionItem!.Status.Equals(FashionItemStatus.PendingForOrder))
        {
            throw new StatusNotAvailableException();
        }

        orderDetail.IndividualFashionItem.Status = itemStatus;
        if (order.OrderDetails.Any(it => it.IndividualFashionItem.Status.Equals(FashionItemStatus.Unavailable)))
        {
            foreach (var detail in order.OrderDetails)
            {
                detail.IndividualFashionItem.Status = FashionItemStatus.Reserved;
                await ScheduleReservedItemEnding(detail.IndividualFashionItem.ItemId);
                // gui mail thong bao 
            }
            order.Status = OrderStatus.Cancelled;
        }
        if (order.OrderDetails.All(c => c.IndividualFashionItem!.Status == FashionItemStatus.OnDelivery))
        {
            order.Status = OrderStatus.OnDelivery;
            await _emailService.SendEmailOrder(order);
        }

        await _orderRepository.UpdateOrder(order);
        
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();
        response.ResultStatus = ResultStatus.Success;
        switch (order.Status)
        {
            case OrderStatus.OnDelivery:
                response.Messages = new[] { "Confirm all items successfully. Order has to be ready for customer." };
                break;
            case OrderStatus.Cancelled:
                response.Messages = new[] { "Order is cancelled." };
                break;
            case OrderStatus.Pending:
                response.Messages = new[] { "Confirm item successfully" };
                break;
        }
        
        response.Data = _mapper.Map<OrderResponse>(order);
        return response;
    }
    private async Task ScheduleReservedItemEnding(Guid itemId)
    {
        var schedule = await _schedulerFactory.GetScheduler();
        var jobDataMap = new JobDataMap()
        {
            { "ItemId", itemId }
        };
        var endJob = JobBuilder.Create<FashionItemReservedEndingJob>()
            .WithIdentity($"EndReservedItem_{itemId}")
            .SetJobData(jobDataMap)
            .Build();
        var endTrigger = TriggerBuilder.Create()
            .WithIdentity($"EndReservedItemTrigger_{itemId}")
            .StartAt(new DateTimeOffset(DateTime.UtcNow.AddMinutes(5)))
            .Build();
        await schedule.ScheduleJob(endJob, endTrigger);
    }
    public async Task<DotNext.Result<ShippingFeeResult, ErrorCode>> CalculateShippingFee(List<Guid> itemIds,
        int destinationDistrictId)
    {
        var shippingFee = 0m;
        var shopLocation = new HashSet<ShippingLocation>();
        var shops = await _fashionItemRepository.GetIndividualQueryable()
            .Include(x => x.Variation)
            .ThenInclude(x => x.MasterItem)
            .ThenInclude(x => x.Shop)
            .Where(x => itemIds.Contains(x.ItemId))
            .Select(x => new
            {
                ShopId = x.Variation.MasterItem.ShopId,
                Address = x.Variation.MasterItem.Shop.Address,
                GhnDistrictId = x.Variation.MasterItem.Shop.GhnDistrictId,
                GhnWardCode = x.Variation.MasterItem.Shop.GhnWardCode,
                ShopCode = x.Variation.MasterItem.Shop.ShopCode
            })
            .ToListAsync();

        _logger.LogInformation("There is {ShopCount} shops", shops.Count);

        foreach (var shop in shops)
        {
            var ghnShippingResult = await _giaoHangNhanhService
                .CalculateShippingFee(new CalculateShippingRequest()
                {
                    FromDistrictId = shop.GhnDistrictId.Value,
                    ToDistrictId = destinationDistrictId
                });

            if (!ghnShippingResult.IsSuccessful)
            {
                return new Result<ShippingFeeResult, ErrorCode>(ghnShippingResult.Error);
            }

            shippingFee += ghnShippingResult.Value.Data.ServiceFee;
            shopLocation.Add(
                new ShippingLocation()
                {
                    Address = shop.Address,
                    DistrictId = shop.GhnDistrictId.Value,
                    WardCode = int.Parse(shop.GhnWardCode)
                }
            );
        }

        return new DotNext.Result<ShippingFeeResult, ErrorCode>(new ShippingFeeResult
        {
            ShopLocation = shopLocation.ToArray(),
            ShippingDestination = new ShippingLocation()
            {
            },
            ShippingFee = shippingFee,
        });
    }
}