using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ApproveConsignSaleRequest
{
    public Guid? CategoryId { get; set; }
    public int? SalePrice { get; set; }
    public ConsignSaleStatus Status { get; set; }
}