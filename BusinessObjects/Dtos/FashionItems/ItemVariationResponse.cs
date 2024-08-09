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
    public PaginationResponse<IndividualItemResponse> IndividualItemsPagination { get; set; }
}

