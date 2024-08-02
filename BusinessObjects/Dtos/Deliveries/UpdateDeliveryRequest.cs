using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Deliveries;

public class UpdateDeliveryRequest
{
    [Required]
    public string RecipientName { set; get; }
    [Required, Phone]
    public string Phone { set; get; }
    [Required]
    public string Residence { set; get; }
    [Required]
    public string AddressType { set; get; }
    public string IsDefult { get; set; }
}