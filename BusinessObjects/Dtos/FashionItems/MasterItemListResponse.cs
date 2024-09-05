using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemListResponse
{
    public Guid MasterItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ItemCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Brand { get; set; }
    public GenderType Gender { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public bool IsConsignment { get; set; }
    public Guid ShopId { get; set; }
    public string ShopAddress { get; set; }
    public int StockCount { get; set; }
    public int? ItemInStock { get; set; }
    public List<string> Images { get; set; } = [];
}