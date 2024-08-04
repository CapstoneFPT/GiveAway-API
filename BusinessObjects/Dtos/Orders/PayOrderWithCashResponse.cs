namespace BusinessObjects.Dtos.Orders;

public class PayOrderWithCashResponse
{
    public Guid OrderId { get; set; }
    public int AmountGiven { get; set; }
    public OrderResponse Order { get; set; }
    public decimal Change => AmountGiven - Order.TotalPrice;
}