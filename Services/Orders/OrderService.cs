using System.Drawing;
using System.Linq.Expressions;
using AutoMapper;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Repositories.FashionItems;
using Repositories.OrderLineItems;
using Repositories.Orders;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.OrderLineItems;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories.Accounts;
using Repositories.AuctionItems;
using Repositories.Recharges;
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
using System.Text;
using BusinessObjects.Dtos.Shops;
using IronPdf.Rendering;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Transactions;
using Services.VnPayService;

namespace Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFashionItemRepository _fashionItemRepository;
    private readonly IOrderLineItemRepository _orderLineItemRepository;
    private readonly IAuctionItemRepository _auctionItemRepository;
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;
    private readonly IRechargeRepository _rechargeRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IRefundRepository _refundRepository;
    private readonly IGiaoHangNhanhService _giaoHangNhanhService;
    private readonly ILogger<OrderService> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IConsignSaleService _consignSaleService;
    private readonly ITransactionService _transactionService;
    private readonly IVnPayService _vnPayService;

    public OrderService(IOrderRepository orderRepository, IFashionItemRepository fashionItemRepository,
        IMapper mapper, IOrderLineItemRepository orderLineItemRepository, IAuctionItemRepository auctionItemRepository,
        IAccountRepository accountRepository, IRechargeRepository rechargeRepository,
        IShopRepository shopRepository, ITransactionRepository transactionRepository,
        IConfiguration configuration, IEmailService emailService, IRefundRepository refundRepository,
        IGiaoHangNhanhService giaoHangNhanhService, ILogger<OrderService> logger, ISchedulerFactory schedulerFactory,
        IConsignSaleService consignSaleService, ITransactionService transactionService,
        IVnPayService vnPayService)
    {
        _orderRepository = orderRepository;
        _fashionItemRepository = fashionItemRepository;
        _mapper = mapper;
        _orderLineItemRepository = orderLineItemRepository;
        _auctionItemRepository = auctionItemRepository;
        _rechargeRepository = rechargeRepository;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
        _transactionRepository = transactionRepository;
        _configuration = configuration;
        _emailService = emailService;
        _refundRepository = refundRepository;
        _giaoHangNhanhService = giaoHangNhanhService;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
        _consignSaleService = consignSaleService;
        _transactionService = transactionService;
        _vnPayService = vnPayService;
    }

    public async Task<Result<InvoiceResponse, ErrorCode>> GenerateInvoice(Guid orderId, Guid shopId)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                return new Result<InvoiceResponse, ErrorCode>(ErrorCode.NotFound);
            }

            var shop = await _shopRepository.GetShopById(shopId);

            var orderLineItems = await _orderLineItemRepository.GetQueryable()
                .Include(x => x.IndividualFashionItem)
                .ThenInclude(x => x.MasterItem)
                .Where(x => x.Order.OrderId == orderId)
                .Select(x => new OrderLineItemListResponse()
                {
                    ItemName = x.IndividualFashionItem.MasterItem.Name,
                    UnitPrice = x.UnitPrice,
                    ItemCode = x.IndividualFashionItem.ItemCode,
                })
                .ToListAsync();

            var invoiceHtml = await GenerateInvoiceHtml(order, orderLineItems, shop);
            var renderer = new ChromePdfRenderer()
            {
                RenderingOptions = new ChromePdfRenderOptions()
                {
                    CssMediaType = PdfCssMediaType.Print,
                    CustomCssUrl = "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
                }
            };
            var pdf = await renderer.RenderHtmlAsPdfAsync(invoiceHtml);

            var fileName = $"Invoice_{order.OrderCode}.pdf";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            pdf.SaveAs(filePath);

            return new Result<InvoiceResponse, ErrorCode>(new InvoiceResponse
            {
                Content = pdf.BinaryData,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for order {OrderId}", orderId);
            return new Result<InvoiceResponse, ErrorCode>(ErrorCode.ServerError);
        }
    }

    private async Task<string> GenerateInvoiceHtml(Order order, List<OrderLineItemListResponse> orderLineItems,
        ShopDetailResponse shop)
    {
        var templatePath = Path.Combine("InvoiceTemplate", "only-invoice.html");
        var template = await File.ReadAllTextAsync(templatePath);

        template = template.Replace("{{InvoiceNumber}}", order.OrderCode)
            .Replace("{{IssueDate}}", order.CreatedDate.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss"))
            .Replace("{{PaymentMethod}}", order.PaymentMethod.ToString())
            .Replace("{{CustomerName}}", order.RecipientName ?? (order.Member != null ? order.Member.Fullname : "N/A"))
            .Replace("{{CustomerAddress}}", order.Address ?? "N/A")
            .Replace("{{CustomerPhone}}", order.Phone ?? "N/A")
            .Replace("{{CustomerEmail}}", order.Email ?? "N/A")
            .Replace("{{ShopAddress}}", shop.Address ?? "N/A")
            .Replace("{{ShopPhone}}", shop.Phone ?? "N/A");

        var itemsHtml = new StringBuilder();
        foreach (var item in orderLineItems)
        {
            itemsHtml.Append($@"
        <tr>
            <td>{item.ItemCode ?? "N/A"}</td>
            <td>{item.ItemName ?? "N/A"}</td>
            <td class='text-end'>1</td>
            <td class='text-end'>{item.UnitPrice:N0} VND</td>
            <td class='text-end'>{item.UnitPrice:N0} VND</td>
        </tr>");
        }

        template = template.Replace("{{OrderItems}}", itemsHtml.ToString());

        // Replace totals
        template = template.Replace("{{Subtotal}}", $"{orderLineItems.Sum(x => x.UnitPrice):N0}")
            .Replace("{{ShippingFee}}", $"{order.ShippingFee:N0}")
            
            .Replace("{{Total}}", $"{order.TotalPrice:N0}");
        if (order.Discount == 0)
        {
            template = template.Replace("{{Discount}}", $"0");
        }
        else
        {
            template = template.Replace("{{Discount}}", $"-{order.Discount:N0}");
        }

        return template;
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

        var checkItemAvailable =
            await _orderRepository.IsOrderAvailable(cart.CartItems.Select(ci => ci.ItemId).ToList());
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

        var checkOrderExisted =
            await _orderRepository.IsOrderExisted(cart.CartItems.Select(ci => ci.ItemId).ToList(), accountId) ?? [];
        if (checkOrderExisted.Count > 0)
        {
            var listItemExisted = checkOrderExisted.Select(x => x.IndividualFashionItemId.Value).ToList() ?? [];
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

    public async Task<Result<ExcelResponse, ErrorCode>> ExportOrdersToExcel(ExportOrdersToExcelRequest request)
    {
        try
        {
            Expression<Func<Order, bool>> predicate = o => true;

            if (request.OrderCode != null)
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.OrderCode, $"%{request.OrderCode}%"));
            }

            if (request.RecipientName != null)
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.RecipientName, $"%{request.RecipientName}%"));
            }

            if (request.Phone != null)
            {
                predicate = predicate.And(x => x.Phone == request.Phone);
            }

            if (request.Statuses.Length > 0)
            {
                predicate = predicate.And(x => request.Statuses.Contains(x.Status));
            }

            if (request.PaymentMethods.Length > 0)
            {
                predicate = predicate.And(x => request.PaymentMethods.Contains(x.PaymentMethod));
            }

            if (request.StartDate != null)
            {
                predicate = predicate.And(x => x.CreatedDate >= request.StartDate);
            }

            if (request.EndDate != null)
            {
                predicate = predicate.And(x => x.CreatedDate <= request.EndDate);
            }

            if (request.PurchaseTypes.Length > 0)
            {
                predicate = predicate.And(x => request.PurchaseTypes.Contains(x.PurchaseType));
            }

            if (request.MinTotalPrice != null)
            {
                predicate = predicate.And(x => x.TotalPrice >= request.MinTotalPrice.Value);
            }

            if (request.MaxTotalPrice != null)
            {
                predicate = predicate.And(x => x.TotalPrice <= request.MaxTotalPrice.Value);
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(x =>
                    x.OrderLineItems.Any(c => c.IndividualFashionItem.MasterItem.ShopId == request.ShopId));
            }

            var orders = await _orderRepository.GetQueryable()
                .Include(o => o.Member)
                .Include(o => o.OrderLineItems)
                .ThenInclude(oli => oli.IndividualFashionItem)
                .ThenInclude(ifi => ifi.MasterItem)
                .AsSplitQuery()
                .Where(predicate)
                .Select(o => new
                {
                    o.OrderCode,
                    CustomerName = o.Member.Fullname ?? "N/A",
                    o.RecipientName,
                    o.CreatedDate,
                    o.TotalPrice,
                    o.Status,
                    ItemsCount = o.OrderLineItems.Count,
                    o.ShippingFee,
                    o.Discount,
                    o.PaymentMethod,
                    o.PurchaseType,
                    ShopAddresses = o.OrderLineItems.Select(oli => new {
                        Address = oli.IndividualFashionItem.MasterItem.Shop.Address,
                        ShopId = oli.IndividualFashionItem.MasterItem.ShopId
                    }).Distinct().ToList()
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return new Result<ExcelResponse, ErrorCode>(ErrorCode.NotFound);
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

                // Styling
                using (var range = worksheet.Cells["A1:J1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // Headers
                worksheet.Cells[1, 1].Value = "Order Code";
                worksheet.Cells[1, 2].Value = "Customer Name";
                worksheet.Cells[1, 3].Value = "Order Date";
                worksheet.Cells[1, 4].Value = "Total Price";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Payment Method";
                worksheet.Cells[1, 7].Value = "Purchase Type";
                worksheet.Cells[1, 8].Value = "Items Count";
                worksheet.Cells[1, 9].Value = "Shipping Fee";
                worksheet.Cells[1, 10].Value = "Discount";
                worksheet.Cells[1, 11].Value = "Shop Address";

                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cells[row, 1].Value = order.OrderCode;
                    worksheet.Cells[row, 2].Value = order.RecipientName ?? (order.CustomerName != null ?
                        order.CustomerName : "N/A");
                    worksheet.Cells[row, 3].Value = order.CreatedDate.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 4].Value = order.TotalPrice + " VND";
                    worksheet.Cells[row, 5].Value = order.Status.ToString();
                    worksheet.Cells[row, 6].Value = order.PaymentMethod.ToString();
                    worksheet.Cells[row, 7].Value = order.PurchaseType.ToString();
                    worksheet.Cells[row, 8].Value = order.ItemsCount;
                    worksheet.Cells[row, 9].Value = order.ShippingFee + " VND";
                    worksheet.Cells[row, 10].Value = order.Discount + " VND";
                    worksheet.Cells[row, 11].Value = string.Join("\n", order.ShopAddresses.Select(x => x.Address));
                    // Alternate row colors
                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 11].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    row++;
                }

                // Formatting
                worksheet.Cells[2, 4, row - 1, 4].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[2, 5, row - 1, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[2, 8, row - 1, 11].Style.Numberformat.Format = "#,##0.00";

                // Auto-fit columns
                worksheet.Cells[1, 1, row - 1, 11].AutoFitColumns();

                // Add borders
                var borderStyle = worksheet.Cells[1, 1, row - 1, 11].Style.Border;
                borderStyle.Top.Style = borderStyle.Left.Style =
                    borderStyle.Right.Style = borderStyle.Bottom.Style = ExcelBorderStyle.Thin;

                // Add title
                worksheet.InsertRow(1, 2);
                worksheet.Cells["A1:J1"].Merge = true;
                worksheet.Cells["A1"].Value = request.ShopId.HasValue ? $"Order Report for {orders.FirstOrDefault()?.ShopAddresses.Where(x => x.ShopId == request.ShopId).FirstOrDefault()?.Address}" : "Order Report";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var content = await package.GetAsByteArrayAsync();

                return new Result<ExcelResponse, ErrorCode>(new ExcelResponse
                {
                    Content = content,
                    FileName = $"Orders_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orders to Excel");
            return new Result<ExcelResponse, ErrorCode>(ErrorCode.ServerError);
        }
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
                new OrderLineItem()
                {
                    OrderId = orderResult.OrderId,
                    IndividualFashionItemId = orderRequest.AuctionFashionItemId,
                    UnitPrice = orderRequest.TotalPrice,
                    CreatedDate = DateTime.UtcNow,
                }
            ;
        var orderDetailResult =
            await _orderLineItemRepository.CreateOrderLineItem(orderDetails);

        orderResult.OrderLineItems = new List<OrderLineItem>() { orderDetailResult };
        return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
        {
            Data = _mapper.Map<Order, OrderResponse>(orderResult),
            ResultStatus = ResultStatus.Success
        };
    }

    public async Task<DotNext.Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>>
        GetOrderLineItemByOrderId(Guid orderId, OrderLineItemRequest request)
    {
        try
        {
            return await _orderLineItemRepository.GetAllOrderLineItemsByOrderId(orderId, request);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when get order line item by order id");
            return new Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>(ErrorCode.ServerError);
        }
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
        var orderDetails = await _orderLineItemRepository.GetOrderLineItems(x => x.OrderId == orderOrderId);
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

    public async Task<Order?> GetOrderById(Guid orderId)
    {
        var result = await _orderRepository.GetSingleOrder(x => x.OrderId == orderId);
        return result;
    }

    public async Task UpdateOrder(Order order)
    {
        await _orderRepository.UpdateOrder(order);
    }

    public async Task<DotNext.Result<PaginationResponse<OrderListResponse>, ErrorCode>> GetOrdersByAccountId(
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
            MemberId = order.MemberId,
            CompletedDate = order.CompletedDate,
            ContactNumber = order.Phone,
            RecipientName = order.RecipientName,
            PurchaseType = order.PurchaseType,
            Address = order.Address,
            PaymentMethod = order.PaymentMethod,
            CustomerName = order.Member != null ? order.Member.Fullname : "N/A",
            Email = order.Email,
            Subtotal = order.OrderLineItems.Sum(x => x.UnitPrice * x.Quantity),
            Quantity = order.OrderLineItems.Count,
            AuctionTitle = order.Bid != null ? order.Bid.Auction.Title : "N/A",
            ShippingFee = order.ShippingFee,
            Discount = order.Discount,
            IsAuctionOrder = order.BidId != null
        };


        if (request.Status != null)
        {
            predicate = predicate.And(order => order.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.OrderCode))
        {
            predicate = predicate.And(order => EF.Functions.ILike(order.OrderCode, $"%{request.OrderCode}%"));
        }

        if (request.ShopId.HasValue)
        {
            predicate = predicate.And(order =>
                order.OrderLineItems.Any(c =>
                    c.IndividualFashionItem.MasterItem.ShopId == request.ShopId.Value));
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

        (List<OrderListResponse> Items, int Page, int PageSize, int TotalCount) =
            await _orderRepository.GetOrdersProjection<OrderListResponse>(request.PageNumber,
                request.PageSize, predicate, selector);

        return new Result<PaginationResponse<OrderListResponse>, ErrorCode>(new PaginationResponse<OrderListResponse>()
        {
            Items = Items,
            PageNumber = request.PageNumber ?? -1,
            PageSize = request.PageSize ?? -1,
            TotalCount = TotalCount
        });
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
            admin.Balance += order.TotalPrice;
            await _accountRepository.UpdateAccount(admin);
            var transaction = new Transaction()
            {
                OrderId = orderId,
                ReceiverId = order.MemberId,
                ReceiverBalance = order.Member.Balance,
                SenderBalance = admin.Balance,
                SenderId = admin.AccountId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.RefundProduct,
                PaymentMethod = PaymentMethod.Point
            };
            await _transactionRepository.CreateTransaction(transaction);
        }

        order.Status = OrderStatus.Cancelled;
        foreach (var item in order.OrderLineItems.Select(c => c.IndividualFashionItem))
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
            admin.Balance += order.TotalPrice;
            await _accountRepository.UpdateAccount(admin);
            var transaction = new Transaction()
            {
                OrderId = orderId,
                ReceiverId = order.MemberId,
                SenderId = admin.AccountId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.RefundProduct
            };
            await _transactionRepository.CreateTransaction(transaction);
        }
        else
        {
            throw new StatusNotAvailableException();
        }

        order.Status = OrderStatus.Cancelled;
        foreach (var item in order.OrderLineItems.Select(c => c.IndividualFashionItem))
        {
            item.Status = FashionItemStatus.Unavailable;
        }

        await _orderRepository.UpdateOrder(order);
        /*await _emailService.SendEmailCancelOrderByShop(order);*/
        response.Messages = ["This order is cancelled by shop for some reason."];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }

    private Expression<Func<Order, bool>> GetOrderListPredicate(OrderRequest orderRequest)
    {
        Expression<Func<Order, bool>> predicate = order => true;
        if (orderRequest.Status != null)
        {
            predicate = predicate.And(order => order.Status == orderRequest.Status);
        }

        if (!string.IsNullOrEmpty(orderRequest.OrderCode))
        {
            predicate = predicate.And(order => EF.Functions.ILike(order.OrderCode, $"%{orderRequest.OrderCode}%"));
        }

        if (orderRequest.ShopId.HasValue)
        {
            predicate = predicate.And(order =>
                order.OrderLineItems.Any(c => c.IndividualFashionItem.MasterItem.ShopId == orderRequest.ShopId)
            );
        }

        if (orderRequest.PaymentMethod != null)
        {
            predicate = predicate.And(order => order.PaymentMethod == orderRequest.PaymentMethod);
        }

        if (orderRequest.PurchaseType != null)
        {
            predicate = predicate.And(order => order.PurchaseType == orderRequest.PurchaseType);
        }

        if (orderRequest.IsFromAuction == true)
        {
            predicate = predicate.And(ord => ord.BidId != null);
        }

        if (orderRequest.IsFromAuction == false)
        {
            predicate = predicate.And(ord => ord.BidId == null);
        }

        if (orderRequest.Email != null)
        {
            predicate = predicate.And(order =>
                order.Email != null && EF.Functions.ILike(order.Email, $"%{orderRequest.Email}%"));
        }

        if (orderRequest.RecipientName != null)
        {
            predicate = predicate.And(order =>
                order.RecipientName != null &&
                EF.Functions.ILike(order.RecipientName, $"%{orderRequest.RecipientName}%"));
        }

        if (orderRequest.Phone != null)
        {
            predicate = predicate.And(order =>
                order.Phone != null && EF.Functions.ILike(order.Phone, $"%{orderRequest.Phone}%"));
        }

        if (orderRequest.CustomerName != null)
        {
            predicate = predicate.And(order =>
                order.Member != null && EF.Functions.ILike(order.Member.Fullname, $"%{orderRequest.CustomerName}%"));
        }

        return predicate;
    }

    public async Task<DotNext.Result<PaginationResponse<OrderListResponse>, ErrorCode>> GetOrders(
        OrderRequest orderRequest)
    {
        var predicate = GetOrderListPredicate(orderRequest);
        Expression<Func<Order, OrderListResponse>> selector = order => new OrderListResponse()
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedDate = order.CreatedDate,
            PaymentDate = order.OrderLineItems.Select(x => x.PaymentDate).Max(),
            MemberId = order.MemberId,
            CompletedDate = order.CompletedDate,
            ContactNumber = order.Phone,
            RecipientName = order.RecipientName,
            PurchaseType = order.PurchaseType,
            Subtotal = order.OrderLineItems.Sum(x => x.UnitPrice * x.Quantity),
            ShippingFee = order.ShippingFee,
            Discount = order.Discount,
            Address = order.Address,
            PaymentMethod = order.PaymentMethod,
            CustomerName = order.Member != null ? order.Member.Fullname : order.RecipientName ?? "N/A",
            Email = order.Email,
            Quantity = order.OrderLineItems.Count,
            AuctionTitle = order.Bid != null ? order.Bid.Auction.Title : "N/A",
        };


        (List<OrderListResponse> Items, int Page, int PageSize, int TotalCount) =
            await _orderRepository.GetOrdersProjection<OrderListResponse>(orderRequest.PageNumber,
                orderRequest.PageSize, predicate, selector);

        var response = new PaginationResponse<OrderListResponse>()
        {
            Items = Items,
            PageNumber = Page,
            PageSize = PageSize,
            TotalCount = TotalCount,
            SearchTerm = orderRequest.OrderCode,
        };

        return new Result<PaginationResponse<OrderListResponse>, ErrorCode>(response);
    }


    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId,
        Guid orderId)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();

        var order = await _orderRepository.GetSingleOrder(c => c.OrderId == orderId);
        if (order is null)
        {
            throw new OrderNotFoundException();
        }

        var orderDetailFromShop = order!.OrderLineItems
            .Where(c => c.IndividualFashionItem.MasterItem.ShopId == shopId).ToList();
        if (orderDetailFromShop.Count == 0)
        {
            throw new OrderDetailNotFoundException();
        }

        foreach (var orderDetail in orderDetailFromShop)
        {
            var fashionItem = orderDetail.IndividualFashionItem;
            if (fashionItem is { Status: FashionItemStatus.OnDelivery })
            {
                fashionItem.Status = FashionItemStatus.Refundable;
                orderDetail.RefundExpirationDate = DateTime.UtcNow.AddMinutes(15);
                if (order.PaymentMethod.Equals(PaymentMethod.COD))
                    orderDetail.PaymentDate = DateTime.UtcNow;
                await ScheduleRefundableItemEnding(fashionItem.ItemId, orderDetail.RefundExpirationDate.Value);
            }
            else
            {
                throw new FashionItemNotFoundException();
            }
        }

        if (order.OrderLineItems.All(c => c.IndividualFashionItem.Status.Equals(FashionItemStatus.Refundable)))
        {
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.UtcNow;
            if (order.PaymentMethod is PaymentMethod.COD)
            {
                var transaction = new Transaction()
                {
                    OrderId = order.OrderId,
                    CreatedDate = DateTime.UtcNow,
                    PaymentMethod = PaymentMethod.COD,
                    SenderId = order.MemberId,
                    Amount = order.TotalPrice,
                    SenderBalance = order.Member?.Balance ?? 0,
                    Type = TransactionType.Purchase,
                };
                await _transactionRepository.CreateTransaction(transaction);
            }
        }

        await _orderRepository.UpdateOrder(order);

        response.Data = _mapper.Map<OrderResponse>(order);
        if (order.Status.Equals(OrderStatus.Completed))
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

    private async Task ScheduleRefundableItemEnding(Guid itemId, DateTime expiredTime)
    {
        var schedule = await _schedulerFactory.GetScheduler();
        var jobDataMap = new JobDataMap()
        {
            { "RefundItemId", itemId }
        };
        var endJob = JobBuilder.Create<FashionItemRefundEndingJob>()
            .WithIdentity($"EndRefundableItem_{itemId}")
            .SetJobData(jobDataMap)
            .Build();
        var endTrigger = TriggerBuilder.Create()
            .WithIdentity($"EndRefundableItemTrigger_{itemId}")
            .StartAt(new DateTimeOffset(expiredTime))
            .Build();
        await schedule.ScheduleJob(endJob, endTrigger);
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrderByShop(Guid shopId,
        CreateOrderRequest request)
    {
        var response = new BusinessObjects.Dtos.Commons.Result<OrderResponse>();
        if (request.ItemIds.Count == 0)
        {
            response.Messages = ["You have no item for order"];
            response.ResultStatus = ResultStatus.Error;
            return response;
        }


        var checkItemAvailable = await _orderRepository.IsOrderAvailable(request.ItemIds);
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

        var isitembelongshop = await _fashionItemRepository.IsItemBelongShop(shopId, request.ItemIds);
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

        var listItem = await _fashionItemRepository.GetIndividualQueryable()
            .Where(c => request.ItemIds.Contains(c.ItemId)).ToListAsync();
        var isMember = await _accountRepository.FindUserByPhone(request.Phone);
        Order order = new Order()
        {
            PurchaseType = PurchaseType.Offline,
            PaymentMethod = PaymentMethod.Cash,
            Address = request.Address,
            RecipientName = request.RecipientName,
            Phone = request.Phone,
            Email = request.Email,
            Status = OrderStatus.Completed,
            Discount = request.Discount,
            CreatedDate = DateTime.UtcNow,
            CompletedDate = DateTime.UtcNow,
            TotalPrice = listItem.Sum(c => c.SellingPrice!.Value),
            OrderCode = _orderRepository.GenerateUniqueString(),
            MemberId = isMember?.AccountId
        };
        await _orderRepository.CreateOrder(order);
        var listOrderLineItem = new List<OrderLineItem>();
        foreach (var item in listItem)
        {
            OrderLineItem orderLineItem = new OrderLineItem();
            orderLineItem.OrderId = order.OrderId;
            orderLineItem.UnitPrice = item.SellingPrice!.Value;
            orderLineItem.CreatedDate = DateTime.UtcNow;
            orderLineItem.Quantity = 1;
            orderLineItem.PaymentDate = DateTime.UtcNow;
            orderLineItem.IndividualFashionItemId = item.ItemId;
            listOrderLineItem.Add(orderLineItem);

            item.Status = FashionItemStatus.Refundable;
            await _fashionItemRepository.UpdateFashionItem(item);
            await _orderLineItemRepository.CreateOrderLineItem(orderLineItem);
        }

        var orderTransaction = new Transaction()
        {
            OrderId = order.OrderId,
            CreatedDate = DateTime.UtcNow,
            Type = TransactionType.Purchase,
            Amount = order.TotalPrice,
            ShopId = shopId,
            SenderId = isMember?.AccountId,
            SenderBalance = isMember?.Balance ?? 0,
            PaymentMethod = PaymentMethod.Cash,
        };
        await _transactionRepository.CreateTransaction(orderTransaction);
        response.Data = new OrderResponse()
        {
            OrderId = order.OrderId,
            Quantity = listOrderLineItem.Count,
            TotalPrice = order.TotalPrice,
            CreatedDate = order.CreatedDate,
            Address = order.Address,
            ContactNumber = order.Phone,
            RecipientName = order.RecipientName,
            Email = order.Email,
            PaymentMethod = order.PaymentMethod,
            PurchaseType = order.PurchaseType,
            CompletedDate = order.CompletedDate,
            Discount = order.Discount,
            OrderCode = order.OrderCode,
            Status = order.Status,
            OrderLineItems = listOrderLineItem.Select(c => new OrderLineItemDetailedResponse()
            {
                OrderLineItemId = c.OrderLineItemId,
                UnitPrice = c.UnitPrice,
                CreatedDate = c.CreatedDate,
                Quantity = c.Quantity,
                PaymentDate = c.PaymentDate,
            }).ToList()
        };
        response.Messages = ["Create Successfully"];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }

    public async Task<Result<PayOrderOfflineResponse, ErrorCode>> OfflinePay(Guid shopId, Guid orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);

        if (order!.OrderLineItems.All(c => c.PaymentDate != null))
        {
            throw new InvalidOperationException("Order Already Paid");
        }

        if (order.PaymentMethod != PaymentMethod.Cash)
        {
            throw new InvalidOperationException("This order can only be paid with cash");
        }

        order.Status = OrderStatus.Completed;
        order.CompletedDate = DateTime.UtcNow;

        var orderLineItems = order.OrderLineItems;
        foreach (var item in orderLineItems)
        {
            item.RefundExpirationDate = DateTime.UtcNow.AddDays(7);
            item.PaymentDate = DateTime.UtcNow;
            item.IndividualFashionItem.Status = FashionItemStatus.Refundable;
        }

        await _orderRepository.UpdateOrder(order);

        var shop = await _shopRepository.GetSingleShop(x => x.ShopId == shopId);
        var shopAccount = await _accountRepository.GetAccountById(shop!.StaffId);
        shopAccount!.Balance += order.TotalPrice;
        await _accountRepository.UpdateAccount(shopAccount);

        var transaction = new Transaction()
        {
            OrderId = orderId,
            CreatedDate = DateTime.UtcNow,
            ShopId = shopId,
            Type = TransactionType.CustomerSale,
            Amount = order.TotalPrice,
        };

        await _transactionRepository.CreateTransaction(transaction);

        var response = new PayOrderOfflineResponse
        {
            OrderId = order.OrderId,
            Quantity = order.OrderLineItems.Count,
            OrderCode = order.OrderCode,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            CreatedDate = order.CreatedDate,
            Address = order.Address ?? "N/A",
            TotalPrice = order.TotalPrice,
            CompletedDate = order.CompletedDate,
            Phone = order.Phone ?? "N/A",
            ReciepientName = order.RecipientName ?? "N/A",
            PurchaseType = order.PurchaseType,
            Email = order.Email ?? "N/A",
            Discount = order.Discount,
            ShippingFee = order.ShippingFee,
            PaymentDate = order.CreatedDate,
            Subtotal = order.OrderLineItems.Sum(x => x.UnitPrice * x.Quantity),
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

        _logger.LogInformation("Update Admin Balance: {Balance} + {TotalPrice}", account.Balance, order.TotalPrice);
        account!.Balance -= order.TotalPrice;
        _logger.LogInformation("Update Admin Balance After: {Balance}", account.Balance);
        await _accountRepository.UpdateAccount(account);
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmPendingOrder(Guid orderdetailId,
        ConfirmPendingOrderRequest itemStatus)
    {
        var order = await _orderRepository.GetSingleOrder(c =>
            c.OrderLineItems.Any(c => c.OrderLineItemId == orderdetailId));
        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (!order.Status.Equals(OrderStatus.Pending))
        {
            throw new StatusNotAvailableWithMessageException("This order is not Pending");
        }

        if (!itemStatus.ItemStatus.Equals(FashionItemStatus.ReadyForDelivery) &&
            !itemStatus.ItemStatus.Equals(FashionItemStatus.Unavailable))
        {
            throw new StatusNotAvailableWithMessageException("You can only set OnDelivery or Unavailable");
        }

        var orderDetail = order.OrderLineItems.FirstOrDefault(c => c.OrderLineItemId == orderdetailId);


        if (orderDetail == null)
        {
            throw new OrderDetailNotFoundException();
        }

        if (!orderDetail.IndividualFashionItem!.Status.Equals(FashionItemStatus.PendingForOrder))
        {
            throw new StatusNotAvailableWithMessageException("This item status is not PendingForOrder");
        }

        orderDetail.IndividualFashionItem.Status = itemStatus.ItemStatus;
        if (order.OrderLineItems.Any(it => it.IndividualFashionItem.Status.Equals(FashionItemStatus.Unavailable)))
        {
            foreach (var detail in order.OrderLineItems.Where(c =>
                         c.IndividualFashionItem.Status == FashionItemStatus.ReadyForDelivery))
            {
                detail.IndividualFashionItem.Status = FashionItemStatus.Reserved;
                detail.ReservedExpirationDate = DateTime.UtcNow.AddDays(3);
                await ScheduleReservedItemEnding(detail.IndividualFashionItem.ItemId,
                    detail.ReservedExpirationDate.Value);
                // gui mail thong bao 
            }
        }

        if (order.OrderLineItems.All(it => it.IndividualFashionItem.Status != FashionItemStatus.PendingForOrder) &&
            order.OrderLineItems.Any(it => it.IndividualFashionItem.Status.Equals(FashionItemStatus.Unavailable)))
        {
            order.Status = OrderStatus.Cancelled;
            await _emailService.SendEmailCancelOrderAndReservedItems(order);
        }

        /*if (order.PaymentMethod == PaymentMethod.COD)
        {
            await _emailService.SendEmailOrder(order);
        }*/

        if (order.OrderLineItems.All(c => c.IndividualFashionItem.Status == FashionItemStatus.ReadyForDelivery))
        {
            foreach (var orderLineItem in order.OrderLineItems)
            {
                orderLineItem.IndividualFashionItem.Status = FashionItemStatus.OnDelivery;
            }
        }

        if (order.OrderLineItems.All(c => c.IndividualFashionItem.Status == FashionItemStatus.OnDelivery))
        {
            order.Status = OrderStatus.OnDelivery;
        }

        if (order.Status.Equals(OrderStatus.Cancelled) && !order.PaymentMethod.Equals(PaymentMethod.COD))
        {
            order.Member!.Balance += order.TotalPrice;
            var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
            if (admin == null)
                throw new AccountNotFoundException();
            admin.Balance += order.TotalPrice;
            await _accountRepository.UpdateAccount(admin);
            var transaction = new Transaction()
            {
                OrderId = order.OrderId,
                ReceiverBalance = order.Member.Balance,
                SenderBalance = admin.Balance,
                ReceiverId = order.MemberId,
                SenderId = admin.AccountId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.Point,
                Type = TransactionType.RefundProduct
            };
            await _transactionRepository.CreateTransaction(transaction);
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

    private async Task ScheduleReservedItemEnding(Guid itemId, DateTime reservedExpiration)
    {
        var schedule = await _schedulerFactory.GetScheduler();
        var jobDataMap = new JobDataMap()
        {
            { "ReservedItemId", itemId }
        };
        var endJob = JobBuilder.Create<FashionItemReservedEndingJob>()
            .WithIdentity($"EndReservedItem_{itemId}")
            .SetJobData(jobDataMap)
            .Build();
        var endTrigger = TriggerBuilder.Create()
            .WithIdentity($"EndReservedItemTrigger_{itemId}")
            .StartAt(new DateTimeOffset(reservedExpiration))
            .Build();
        await schedule.ScheduleJob(endJob, endTrigger);
    }

    public async Task<DotNext.Result<ShippingFeeResult, ErrorCode>> CalculateShippingFee(List<Guid> itemIds,
        int destinationDistrictId)
    {
        var shippingFee = 0m;
        var shopLocation = new HashSet<ShippingLocation>();
        var shops = await _fashionItemRepository.GetIndividualQueryable()
            .Include(x => x.MasterItem)
            .ThenInclude(x => x.Shop)
            .Where(x => itemIds.Contains(x.ItemId))
            .Select(x => new
            {
                ShopId = x.MasterItem.ShopId,
                Address = x.MasterItem.Shop.Address,
                GhnDistrictId = x.MasterItem.Shop.GhnDistrictId,
                GhnWardCode = x.MasterItem.Shop.GhnWardCode,
                ShopCode = x.MasterItem.Shop.ShopCode
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

            if (ghnShippingResult.Value.Data != null)
                shippingFee += (int)Math.Round(ghnShippingResult.Value.Data.ServiceFee / 1000) * 1000;
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

    public async Task<Result<OrderDetailedResponse, ErrorCode>> GetDetailedOrder(Guid orderId)
    {
        var query = _orderRepository.GetQueryable();

        Expression<Func<Order, bool>> predicate = order => order.OrderId == orderId;
        Expression<Func<Order, OrderDetailedResponse>> selector = order => new OrderDetailedResponse()
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            PaymentMethod = order.PaymentMethod,
            PurchaseType = order.PurchaseType,
            Address = order.Address ?? "N/A",
            CompletedDate = order.CompletedDate,
            Discount = order.Discount,
            Status = order.Status,
            Email = order.Email ?? "N/A",
            CustomerName = order.Member != null ? order.Member.Fullname : "N/A",
            ShippingFee = order.ShippingFee,
            PaymentDate = order.OrderLineItems.Select(x => x.PaymentDate).Max(),
            TotalPrice = order.TotalPrice,
            MemberId = order.MemberId ?? Guid.Empty,
            Phone = order.Phone ?? "N/A",
            BidId = order.BidId ?? Guid.Empty,
            AddressType = order.AddressType != null ? order.AddressType.Value : default,
            Subtotal = order.OrderLineItems.Sum(x => x.UnitPrice * x.Quantity),
            BidAmount = order.Bid != null ? order.Bid.Amount : 0m,
            AuctionTitle = order.Bid != null ? order.Bid.Auction.Title : "N/A",
            Quantity = order.OrderLineItems.Sum(x => x.Quantity),
            ReciepientName = order.RecipientName ?? "N/A",
            BidCreatedDate = order.Bid != null ? order.Bid.CreatedDate : null,
            CreatedDate = order.CreatedDate
        };
        try
        {
            var result = await query.Include(x => x.OrderLineItems)
                .Include(x => x.Member)
                .Include(x => x.Bid)
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new Result<OrderDetailedResponse, ErrorCode>(ErrorCode.NotFound);
            }

            return new Result<OrderDetailedResponse, ErrorCode>(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetDetailedOrder error");
            return new Result<OrderDetailedResponse, ErrorCode>(ErrorCode.ServerError);
        }
    }

    public async Task<DotNext.Result<VnPayPurchaseResponse, ErrorCode>> PurchaseOrder(Guid orderId,
        PurchaseOrderRequest request)
    {
        var order = await GetOrderById(orderId);

        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (order.PaymentMethod != PaymentMethod.Banking)
        {
            throw new WrongPaymentMethodException("Order is not paid by Banking");
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            throw new InvalidOperationException("Order is not awaiting payment");
        }

        if (order.MemberId != request.MemberId)
        {
            throw new NotAuthorizedToPayOrderException();
        }

        var paymentUrl = _vnPayService.CreatePaymentUrl(
            order.OrderId,
            order.TotalPrice,
            $"{orderId}", "orders");

        return new VnPayPurchaseResponse { PaymentUrl = paymentUrl };
    }

    public async Task<DotNext.Result<string, ErrorCode>> PaymentReturn(IQueryCollection requestParams)
    {
        var response = _vnPayService.ProcessPayment(requestParams);
        var order = await GetOrderById(new Guid(response.OrderId));
        var returnUrl = _configuration.GetSection("RedirectUrl").Value + "process-payment";

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            throw new InvalidOperationException("Order is not awaiting payment");
        }

        if (response.Success)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogWarning("Order not found for OrderCode: {OrderId}", response.OrderId);
                    return new Result<string, ErrorCode>(ErrorCode.NotFound);
                }

                if (order.Status != OrderStatus.AwaitingPayment)
                {
                    _logger.LogWarning("Order already processed: {OrderId}", response.OrderId);
                    return new Result<string, ErrorCode>(ErrorCode.OrderAlreadyProcessed);
                }

                var transaction =
                    await _transactionService.CreateTransactionFromVnPay(response, TransactionType.Purchase);

                if (transaction.ResultStatus == ResultStatus.Success)
                {
                    order.Status = OrderStatus.Pending;
                    foreach (var orderDetail in order.OrderLineItems)
                    {
                        orderDetail.PaymentDate = DateTime.UtcNow;
                    }

                    await UpdateOrder(order);
                    await UpdateFashionItemStatus(order.OrderId);
                    // await _emailService.SendEmailOrder(order);

                    return new Result<string, ErrorCode>(
                        $"{returnUrl}?paymentstatus=success&message={Uri.EscapeDataString("Payment success")}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new Result<string, ErrorCode>(
                    $"{returnUrl}?paymentstatus=error&message={Uri.EscapeDataString(e.Message)}");
            }
        }

        _logger.LogWarning(
            "Payment failed. OrderCode: {OrderId}, ResponseCode: {VnPayResponseCode}", response.OrderId,
            response.VnPayResponseCode);

        return new Result<string, ErrorCode>(
            $"{returnUrl}?paymentstatus=error&message={Uri.EscapeDataString("Payment failed")}");
    }

    public async Task<DotNext.Result<PayWithPointsResponse, ErrorCode>> PurchaseOrderWithPoints(Guid orderId,
        PurchaseOrderRequest request)
    {
        var order = await GetOrderById(orderId);

        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (order.PaymentMethod != PaymentMethod.Point)
        {
            throw new WrongPaymentMethodException("Order is not paid by Point");
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            throw new InvalidOperationException("Order is not awaiting payment");
        }

        if (order.MemberId != request.MemberId)
        {
            throw new NotAuthorizedToPayOrderException();
        }

        if (order.Member!.Balance < order.TotalPrice)
        {
            throw new BalanceIsNotEnoughException(ErrorCode.PaymentFailed);
        }

        foreach (var orderDetail in order.OrderLineItems)
        {
            orderDetail.PaymentDate = DateTime.UtcNow;
        }

        order.Status = OrderStatus.Pending;
        order.Member!.Balance -= order.TotalPrice;

        await UpdateOrder(order);
        await UpdateFashionItemStatus(order.OrderId);
        await UpdateAdminBalance(order);
        await _consignSaleService.UpdateConsignPrice(order.OrderId);
        await _transactionService.CreateTransactionFromPoints(order, request.MemberId, TransactionType.Purchase);
        await _emailService.SendEmailOrder(order);

        return new PayWithPointsResponse()
            { Sucess = true, Message = "Payment success", OrderId = order.OrderId };
    }


    public async Task<DotNext.Result<PayWithPointsResponse, ErrorCode>> CheckoutAuction(Guid orderId,
        CheckoutAuctionRequest request)
    {
        var order = await GetOrderById(orderId);

        if (order == null)
        {
            throw new OrderNotFoundException();
        }

        if (order.PaymentMethod != PaymentMethod.Point)
        {
            throw new WrongPaymentMethodException("Order is not paid by Point");
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            throw new InvalidOperationException("Order is not awaiting payment");
        }

        if (order.MemberId != request.MemberId)
        {
            throw new NotAuthorizedToPayOrderException();
        }

        if (order.Member!.Balance < order.TotalPrice)
        {
            throw new BalanceIsNotEnoughException(ErrorCode.PaymentFailed);
        }

        foreach (var orderDetail in order.OrderLineItems)
        {
            orderDetail.PaymentDate = DateTime.UtcNow;
        }

        order.GhnDistrictId = request.GhnDistrictId;
        order.GhnWardCode = request.GhnWardCode;
        order.GhnProvinceId = request.GhnProvinceId;
        order.Address = request.Address;
        order.RecipientName = request.RecipientName;
        order.Phone = request.Phone;
        order.ShippingFee = request.ShippingFee;
        // order.Discount = request.Discount;
        order.Status = OrderStatus.Pending;
        order.TotalPrice = order.TotalPrice + request.ShippingFee - order.Discount;
        order.Member!.Balance -= order.TotalPrice;

        await UpdateOrder(order);
        await UpdateFashionItemStatus(order.OrderId);
        await UpdateAdminBalance(order);
        await _consignSaleService.UpdateConsignPrice(order.OrderId);
        await _transactionService.CreateTransactionFromPoints(order, request.MemberId, TransactionType.Purchase);
        await _emailService.SendEmailOrder(order);

        return new PayWithPointsResponse()
            { Sucess = true, Message = "Payment success", OrderId = order.OrderId };
    }
}