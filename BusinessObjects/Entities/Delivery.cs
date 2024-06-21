using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Delivery
{
    [Key]
    public Guid DeliveryId { set; get; }
    public string RecipientName { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public AddressType AddressType { set; get; }
    public Account Member { set; get; }
    public Guid MemberId { set; get; }
}

