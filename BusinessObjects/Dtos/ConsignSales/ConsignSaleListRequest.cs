using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ConsignSaleListRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Guid? ShopId { get; set; }
    public string? ConsignSaleCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ConsignSaleStatus? Status { get; set; } 
    public ConsignSaleType? ConsignType { get; set; } 
    public string? Email { get; set; } 
    public string? ConsignorName { get; set; } 
    public string? ConsignorPhone { get; set; }
}