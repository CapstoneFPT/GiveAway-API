using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class FrontPageMasterItemRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public GenderType? GenderType { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public bool IsLeftInStock { get; set; } = true;
}