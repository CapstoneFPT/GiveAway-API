using System.ComponentModel.DataAnnotations;
using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Accounts.Orders.PlaceOrder;

public record PlaceOrderRequest
{
    public PaymentMethod PaymentMethod { get; set; }
    [Required] public string Address { get; set; }
    public int? GhnDistrictId { get; set; }
    public int? GhnWardCode { get; set; }
    public int? GhnProvinceId { get; set; }
    public AddressType? AddressType { get; set; }
    public string RecipientName { get; set; }
    [Phone] public string? Phone { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public List<Guid> CartItems { get; set; } = [];
}