using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class    IndividualItemRequest
{
    public string? SearchItemCode { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? MemberId { get; set; }
    public decimal? MinSellingPrice { get; set; }
    public decimal? MaxSellingPrice { get; set; }
    public FashionItemStatus[]? Status { get; set; }
    public FashionItemType[]? Types { get; set; }
    public bool IsForSale { get; set; } = true;
}