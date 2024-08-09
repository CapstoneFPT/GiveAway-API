using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class ItemVariationRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Color { get; set; }
    public SizeType[]? Size { get; set; }
    
}