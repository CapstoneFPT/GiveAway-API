using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class CreateMasterItemForConsignRequest
{
    public required string MasterItemCode { get; set; }
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    public required Guid CategoryId { get; set; }
    
    public required string[] Images { get; set; }
}