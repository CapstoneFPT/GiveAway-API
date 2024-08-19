using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Address
{
    [Key]
    public Guid AddressId { set; get; }
    public string RecipientName { set; get; }
    public string Phone { set; get; }
    public int? GhnDistrictId { set; get; }
    public int? GhnWardCode { set; get; }
    public string Residence { set; get; }
    public AddressType AddressType { set; get; }
    public Member Member { set; get; }
    public Guid MemberId { set; get; }
    public bool IsDefault { set; get; }
    public DateTime CreatedDate { get; set; }
    public int? GhnProvinceId { get; set; }
}

