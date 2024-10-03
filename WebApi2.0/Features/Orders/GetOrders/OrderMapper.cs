using FastEndpoints;

namespace WebApi2._0.Features.Orders.GetOrders;

public class OrderMapper : Mapper<GetOrdersRequest, OrdersListResponse, Domain.Entities.Order>
{
    public override OrdersListResponse FromEntity(Domain.Entities.Order entity)
    {
        return new OrdersListResponse()
        {
            OrderId = entity.OrderId,
            Address = entity.Address,
            Discount = entity.Discount,
            Email = entity.Email,
            CustomerName = entity.Member != null ? entity.Member.Fullname : "N/A",
            Quantity = entity.OrderLineItems.Count,
            Status = entity.Status,
            Subtotal = entity.OrderLineItems.Sum(x => x.UnitPrice * x.Quantity),
            TotalPrice = entity.TotalPrice,
            PaymentMethod = entity.PaymentMethod,
            ShippingFee = entity.ShippingFee,
            OrderCode = entity.OrderCode,
            PaymentDate = entity.OrderLineItems.Select(x => x.PaymentDate).Max(),
            CompletedDate = entity.CompletedDate,
            AuctionTitle = entity.Bid != null ? entity.Bid.Auction.Title : "N/A",
            ContactNumber = entity.Phone,
            CreatedDate = entity.CreatedDate,
            MemberId = entity.MemberId,
            PurchaseType = entity.PurchaseType,
            RecipientName = entity.RecipientName,
            IsAuctionOrder = entity.BidId != null
        };
    }
}