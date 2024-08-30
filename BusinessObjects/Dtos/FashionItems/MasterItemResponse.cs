using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace BusinessObjects.Dtos.FashionItems;

public class MasterItemResponse
{
    public Guid MasterItemId { get; set; }
    public string MasterItemCode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public GenderType Gender { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsConsignment { get; set; }
    public List<string> Images { get; set; } = [];
    public Guid ShopId { get; set; }
    public int StockCount { get; set; }
}