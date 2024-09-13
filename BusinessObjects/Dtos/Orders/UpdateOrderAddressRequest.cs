using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Orders;

public class UpdateOrderAddressRequest
{
    public string Address { get; set; }
    public int? GhnDistrictId { get; set; }
    public int? GhnWardCode { get; set; }
    public int? GhnProvinceId { get; set; }
    public AddressType? AddressType { get; set; }
    public string RecipientName { get; set; }
    [Phone] public string? Phone { get; set; }
    public decimal ShippingFee { get; set; }
}