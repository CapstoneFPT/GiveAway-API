using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemRequest
{
    public string? SearchTerm { get; set; }
    public string? SearchItemCode { get; set; }
    public string? Brand { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public GenderType? GenderType { get; set; }
    public bool? IsConsignment { get; set; }
    public bool? IsLeftInStock { get; set; }
    public bool? IsForSale { get; set; }
    public bool? IsCategoryAvailable { get; set; }
}