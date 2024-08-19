using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Deliveries;

public class UpdateDeliveryRequest
{
    public string? RecipientName { set; get; }
    [Phone] public string? Phone { set; get; }

    public AddressType? AddressType { set; get; }

    public int? GhnProvinceId { set; get; }
    public int? GhnDistrictId { set; get; }
    public int? GhnWardCode { set; get; }
    public string? Residence { set; get; }
    public bool IsDefault { get; set; }
}