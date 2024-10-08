using FastEndpoints;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Products.FashionItems.GetFashionItems;

public sealed class GetFashionItemsMapper : Mapper<GetFashionItemsRequest, FashionItemsListResponse, IndividualFashionItem>
{
    public override FashionItemsListResponse FromEntity(IndividualFashionItem e)
    {
        return new FashionItemsListResponse
        {
            ItemId = e.ItemId,
            MasterItemId = e.MasterItemId,
            ItemCode = e.ItemCode,
            MasterItemCode = e.MasterItem.MasterItemCode,
            Name = e.MasterItem.Name,
            CategoryId = e.MasterItem.CategoryId,
            ShopId = e.MasterItem.ShopId,
            Gender = e.MasterItem.Gender,
            Color = e.Color,
            Size = e.Size,
            Condition = e.Condition,
            Status = e.Status,
            Type = e.Type,
            SellingPrice = e.SellingPrice ?? 0,
            Brand = e.MasterItem.Brand,
            Image = e.Images.FirstOrDefault() != null ? e.Images.First().Url : "N/A",
            Note = e.Note ?? "N/A",
            InitialPrice = (e as IndividualAuctionFashionItem).InitialPrice ?? 0
        };
    }
}