using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class FashionItemListRequest
{
    public string? ItemCode { get; set; }
    public Guid? MemberId { get; set; }
    public GenderType? Gender { get; set; }
    public string? Color { get; set; }
    public SizeType? Size { get; set; }
    public string? Condition { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public FashionItemStatus[] Status { get; set; } = [];
    public FashionItemType[] Type { get; set; } = [];
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public Guid? MasterItemId { get; set; }
    public string? MasterItemCode { get; set; }
}