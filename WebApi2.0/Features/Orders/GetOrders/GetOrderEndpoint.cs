using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Orders.GetOrders;

public sealed class GetOrderEndpoint : Endpoint<GetOrdersRequest, PaginationResponse<OrdersListResponse>, OrderMapper>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetOrderEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("orders");
        AllowAnonymous();
    }

    public override async Task<PaginationResponse<OrdersListResponse>> ExecuteAsync(GetOrdersRequest req,
        CancellationToken ct)
    {
        var predicate = GetOrdersPredicate.GetPredicate(req);

        var query = _dbContext.Orders.AsQueryable();

        query = query.Where(predicate);
        var count = await query.CountAsync(ct);

        query = query.Skip(PaginationUtils.GetSkip(req.PageNumber, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize));

        var data = await query.Select(x => new OrdersListResponse()
        {
            OrderId = x.OrderId,
            Address = x.Address,
            Discount = x.Discount,
            Email = x.Email,
            CustomerName = x.Member != null ? x.Member.Fullname : "N/A",
            Quantity = x.OrderLineItems.Count,
            Status = x.Status,
            Subtotal = x.OrderLineItems.Sum(orderLineItem => orderLineItem.UnitPrice * orderLineItem.Quantity),
            TotalPrice = x.TotalPrice,
            PaymentMethod = x.PaymentMethod,
            ShippingFee = x.ShippingFee,
            OrderCode = x.OrderCode,
            PaymentDate = x.OrderLineItems.Select(orderLineItem => orderLineItem.PaymentDate).Max(),
            CompletedDate = x.CompletedDate,
            AuctionTitle = x.Bid != null ? x.Bid.Auction.Title : "N/A",
            ContactNumber = x.Phone,
            CreatedDate = x.CreatedDate,
            MemberId = x.MemberId,
            PurchaseType = x.PurchaseType,
            RecipientName = x.RecipientName,
            IsAuctionOrder = x.BidId != null
        }).ToListAsync(ct);
        
        var result = new PaginationResponse<OrdersListResponse>()
        {
            Items = data,
            PageNumber = req.PageNumber ?? -1,
            PageSize = req.PageSize ?? -1,
            TotalCount = count
        };

        return result;
    }
}