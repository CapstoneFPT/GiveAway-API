using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Products.MasterItems.GetMasterItems;

public record GetMasterItemsRequest
{
    public string? MasterItemName { get; set; }
    public string? MasterItemCode { get; set; }
    public string? Brand { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public GenderType[]? Genders { get; set; } = [];
    public bool? IsConsignment { get; set; }
    public bool? IsLeftInStock { get; set; }
    public bool? IsForSale { get; set; }
    public bool? IsCategoryAvailable { get; set; }
}