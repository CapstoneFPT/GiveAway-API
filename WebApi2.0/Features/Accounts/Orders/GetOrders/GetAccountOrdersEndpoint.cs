using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Accounts.Orders.GetOrders;

public class GetAccountOrdersEndpoint : Endpoint<GetAccountOrdersRequest, PaginationResponse<AccountOrdersListResponse>>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetAccountOrdersEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("api/accounts/{accountId}/orders");
    }

    public override async Task<PaginationResponse<AccountOrdersListResponse>> ExecuteAsync(GetAccountOrdersRequest req,
        CancellationToken ct)
    {
        var accountId = Route<Guid>("accountId");
        var query = _dbContext.Orders.AsQueryable();

        query = query.Where(x => x.MemberId == accountId)
            .Where(GetAccountOrdersPredicates.GetPredicate(req));

        var count = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip(PaginationUtils.GetSkip(req.PageNumber, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize))
            .Select(order => new AccountOrdersListResponse()
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
            })
            .ToListAsync(ct);

        return new PaginationResponse<AccountOrdersListResponse>()
        {
            Items = data,
            PageNumber = req.PageNumber ?? -1,
            PageSize = req.PageSize ?? -1,
            TotalCount = count
        };
    }
}