using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemRequest
{
    public string? SearchTerm { get; set; }
    public string? SearchItemCode { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public GenderType? GenderType { get; set; }
}