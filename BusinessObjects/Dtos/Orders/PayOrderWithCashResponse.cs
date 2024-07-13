namespace BusinessObjects.Dtos.Orders;

public class PayOrderWithCashResponse
{
    public Guid OrderId { get; set; }
    public int AmountGiven { get; set; }
    public OrderResponse Order { get; set; }
    public int Change => AmountGiven - Order.TotalPrice;
}