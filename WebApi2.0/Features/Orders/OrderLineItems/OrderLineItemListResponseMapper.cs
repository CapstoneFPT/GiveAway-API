using FastEndpoints;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Orders.OrderLineItems;

public sealed class
    OrderLineItemListResponseMapper : Mapper<GetOrderLineItemsRequest, OrderLineItemsListResponse, OrderLineItem>
{
    public override OrderLineItemsListResponse FromEntity(OrderLineItem e)
    {
        return
            new OrderLineItemsListResponse()
            {
                Condition = e.IndividualFashionItem.Condition,
                CreatedDate = e.CreatedDate,
                Quantity = e.Quantity,
                CategoryName = e.IndividualFashionItem.MasterItem.Category.Name,
                ItemBrand = e.IndividualFashionItem.MasterItem.Brand,
                OrderCode = e.Order.OrderCode,
                PaymentDate = e.PaymentDate,
                ItemCode = e.IndividualFashionItem.ItemCode,
                ItemColor = e.IndividualFashionItem.Color,
                ItemGender = e.IndividualFashionItem.MasterItem.Gender,
                ItemId = e.IndividualFashionItemId,
                ItemImage = e.IndividualFashionItem.Images.Select(x => x.Url).ToList(),
                ItemName = e.IndividualFashionItem.MasterItem.Name,
                ItemNote = e.IndividualFashionItem.Note,
                ItemSize = e.IndividualFashionItem.Size,
                ItemStatus = e.IndividualFashionItem.Status,
                ItemType = e.IndividualFashionItem.Type,
                ShopAddress = e.IndividualFashionItem.MasterItem.Shop.Address,
                ShopId = e.IndividualFashionItem.MasterItem.ShopId,
                UnitPrice = e.IndividualFashionItem.SellingPrice ?? 0,
                OrderLineItemId = e.OrderLineItemId,
                RefundExpirationDate = e.RefundExpirationDate,
                ReservedExpirationDate = e.ReservedExpirationDate
            };
    }
}