using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class IndividualItemListResponse
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; }
    public Guid MasterItemId { get; set; }
    public decimal SellingPrice { get; set; }
    public FashionItemStatus Status { get; set; }
    public FashionItemType Type { get; set; }
    public bool IsOrderedYet { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public string Condition { get; set; }
    public string Image { get; set; }
    public DateTime CreatedDate { get; set; }
}