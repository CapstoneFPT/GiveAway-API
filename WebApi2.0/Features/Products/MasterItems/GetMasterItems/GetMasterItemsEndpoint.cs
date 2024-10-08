using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Products.MasterItems.GetMasterItems;

[HttpGet("api/master-items")]
[AllowAnonymous]
public sealed class GetMasterItemsEndpoint : Endpoint<GetMasterItemsRequest, PaginationResponse<MasterItemsListResponse>>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetMasterItemsEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<PaginationResponse<MasterItemsListResponse>> ExecuteAsync(GetMasterItemsRequest req,
        CancellationToken ct)
    {
        var query = _dbContext.MasterFashionItems.AsQueryable();
        
        
        query = query.Where(GetMasterItemsPredicate.GetPredicate(req));
        var count = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.MasterItemCode)
            .Skip(PaginationUtils.GetSkip(req.PageNumber, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize))
            .Select(item => new MasterItemsListResponse
            {
                MasterItemId = item.MasterItemId,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                ItemCode = item.MasterItemCode,
                CreatedDate = item.CreatedDate,
                Brand = item.Brand,
                Gender = item.Gender,
                CategoryId = item.CategoryId,
                IsConsignment = item.IsConsignment,
                ItemInStock = req.IsForSale == true
                    ? item.IndividualFashionItems.Count(c =>
                        c.Status == FashionItemStatus.Available && c.Type != FashionItemType.ConsignedForAuction)
                    : item.IndividualFashionItems.Count(c => c.Status == FashionItemStatus.Available),
                ShopId = item.ShopId,
                ShopAddress = item.Shop.Address,
                StockCount = item.IndividualFashionItems.Count,
                CategoryName = item.Category.Name,
                Images = item.Images.Select(x => x.Url).ToList()
            }).ToListAsync(ct);

        return new PaginationResponse<MasterItemsListResponse>()
        {
            Items = data,
            PageNumber = req.PageNumber ?? -1,
            PageSize = req.PageSize ?? -1,
            TotalCount = count
        };
    }
}