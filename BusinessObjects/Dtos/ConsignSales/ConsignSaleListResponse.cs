using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ConsignSaleListResponse
{
    public Guid ConsignSaleId { get; set; }
    public ConsignSaleType Type { get; set; }
    public string ConsignSaleCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid ShopId { get; set; }
    public Guid? MemberId { get; set; }
    public ConsignSaleStatus Status { get; set; }
    public ConsignSaleMethod ConsignSaleMethod { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal SoldPrice { get; set; }
    public decimal MemberReceivedAmount { get; set; }
    public string? Consginor { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; } 
    public string? Email { get; set; }
}