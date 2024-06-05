using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Delivery
{
    [Key]
    public Guid DeliveryId { set; get; }
    public string RecipientName { set; get; }
    public string PhoneNumeber { set; get; }
    public string Address { set; get; }
    public string AddressType { set; get; }
    public Account Member { set; get; }
    public Guid MemberId { set; get; }
}