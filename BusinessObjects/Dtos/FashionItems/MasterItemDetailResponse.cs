using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemDetailResponse
{
    public Guid MasterItemId { get; set; }
    public string MasterItemCode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public GenderType Gender { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsConsignment { get; set; }
    public int StockCount { get; set; }
    public ICollection<FashionItemImage> Images { get; set; } = [];
}