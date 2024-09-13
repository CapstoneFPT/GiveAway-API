using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class FashionItemList
{
    public Guid ItemId { get; set; }
    public Guid MasterItemId { get; set; }
    
    public Guid ShopId { get; set; }
    public string Brand { get; set; }
    public string Name { get; set; }
    public string ItemCode { get; set; }
    public GenderType Gender { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public string Condition { get; set; }
    public string RetailPrice { get; set; }
    public string Note { get; set; }
    public decimal SellingPrice { get; set; }
    public string Image { get; set; }
    public FashionItemStatus Status { get; set; }
    public FashionItemType Type { get; set; }
    public Guid CategoryId { get; set; }
    public decimal InitialPrice { get; set; }
    public bool IsOrderedYet { get; set; }
}