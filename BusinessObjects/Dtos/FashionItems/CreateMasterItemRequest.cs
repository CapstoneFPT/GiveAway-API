using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class CreateMasterItemRequest
{
    public string ItemCode { get; set; }
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public GenderType Gender { get; set; }
    public string[] Images { get; set; }
    public Guid[] ShopId { get; set; }
}