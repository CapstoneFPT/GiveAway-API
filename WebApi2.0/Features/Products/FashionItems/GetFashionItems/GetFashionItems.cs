using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Products.FashionItems.GetFashionItems;

[FastEndpoints.HttpGet("api/fashionitems")]
[AllowAnonymous]
public class GetFashionItems : Endpoint<GetFashionItemsRequest, PaginationResponse<FashionItemsListResponse>,
    GetFashionItemsMapper>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetFashionItems(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<PaginationResponse<FashionItemsListResponse>> ExecuteAsync(GetFashionItemsRequest req,
        CancellationToken ct)
    {
        var predicate = GetFashionItemsPredicate.GetPredicate(req);
        var query = _dbContext.IndividualFashionItems.AsQueryable();

        query = query.Where(predicate);
        var count = await query.CountAsync(ct);

        query = query
            .Skip(PaginationUtils.GetSkip(req.PageNumber, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize));

        var data = await query
            .Select(x => new FashionItemsListResponse
            {
                ItemId = x.ItemId,
                MasterItemId = x.MasterItemId,
                ItemCode = x.ItemCode,
                MasterItemCode = x.MasterItem.MasterItemCode,
                Name = x.MasterItem.Name,
                CategoryId = x.MasterItem.CategoryId,
                ShopId = x.MasterItem.ShopId,
                Gender = x.MasterItem.Gender,
                Color = x.Color,
                Size = x.Size,
                Condition = x.Condition,
                Status = x.Status,
                Type = x.Type,
                SellingPrice = x.SellingPrice ?? 0,
                Brand = x.MasterItem.Brand,
                Image = x.Images.FirstOrDefault() != null ? x.Images.First().Url : "N/A",
                Note = x.Note ?? "N/A",
                InitialPrice = (x as IndividualAuctionFashionItem).InitialPrice ?? 0
            })
            .ToListAsync(ct);

        var result = new PaginationResponse<FashionItemsListResponse>
        {
            Items = data,
            PageNumber = req.PageNumber ?? -1,
            PageSize = req.PageSize ?? -1,
            TotalCount = count
        };

        return result;
    }
}