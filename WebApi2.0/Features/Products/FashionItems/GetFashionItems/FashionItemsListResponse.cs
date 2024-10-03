using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Products.FashionItems.GetFashionItems;

public class FashionItemsListResponse
{
    public Guid ItemId { get; set; }
    public Guid MasterItemId { get; set; }
    public string MasterItemCode { get; set; }
    public Guid ShopId { get; set; }
    public string Brand { get; set; }
    public string Name { get; set; }
    public string ItemCode { get; set; }
    public GenderType Gender { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public string Condition { get; set; }
    public string Note { get; set; }
    public decimal SellingPrice { get; set; }
    public string Image { get; set; }
    public FashionItemStatus Status { get; set; }
    public FashionItemType Type { get; set; }
    public Guid CategoryId { get; set; }
    public decimal InitialPrice { get; set; }
}