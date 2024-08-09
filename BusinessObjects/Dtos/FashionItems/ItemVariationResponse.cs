using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class ItemVariationResponse
{
    public Guid VariationId { get; set; }
    public Guid MasterItemId { get; set; }
    public string Condition { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public int StockCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<IndividualItemResponse> IndividualItems { get; set; }
}

public class IndividualItemResponse
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; }
    public Guid VariationId { get; set; }
    public string Note { get; set; }
    public decimal SellingPrice { get; set; }
    public FashionItemStatus Status { get; set; }
    public FashionItemType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public string[] Images { get; set; }
}