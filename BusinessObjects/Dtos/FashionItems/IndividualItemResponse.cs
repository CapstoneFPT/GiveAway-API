using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

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
    public List<string> Images { get; set; }
}