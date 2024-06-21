using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class ConsignSale
{
    [Key] public Guid ConsignSaleId { get; set; }
    public ConsignSaleType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ConsignDuration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public ConsignSaleStatus Status { get; set; }
    public int TotalPrice { get; set; }
    public int SoldPrice { get; set; }
    public int MemberReceivedAmount { get; set; }
    public ICollection<ConsignSaleDetail>? ConsignSaleDetails { get; set; } = new List<ConsignSaleDetail>();
}

