using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemResponse
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public GenderType Gender { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsUniversal { get; set; }
    public List<string> Images { get; set; } = [];
    public Guid ShopId { get; set; }
    public ItemVariationResponse? ItemVariationResponse { get; set; }
    public PaginationResponse<ItemVariationResponse>? ListItemVariationResponses { get; set; }
}