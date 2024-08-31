using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ApproveConsignSaleRequest
{
    public string? ResponseFromShop { get; set; }
    public ConsignSaleStatus Status { get; set; }
}