using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Orders.GetOrders;

public class GetOrderEndpoint : Endpoint<GetOrdersRequest, PaginationResponse<OrdersListResponse>, OrderMapper>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetOrderEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/orders");
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
        
        var data = await query.Select(x => Map.FromEntity(x)).ToListAsync(ct);

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