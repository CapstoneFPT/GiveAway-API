namespace WebApi2._0.Features.Orders.OrderLineItems;

public record GetOrderLineItemsRequest
{
    public Guid OrderId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Guid? OrderLineItemId { get; set; }
    public Guid? OrderCode { get; set; }
}