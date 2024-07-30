using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Orders;

public class PayOrderWithCashRequest
{
    [Required]
    public int AmountGiven { get; set; }
}