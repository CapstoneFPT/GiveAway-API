using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSaleLineItems;

public class CreateConsignDetailRequest
{
    public string Note { get; set; }
    public decimal ExpectedPrice { get; set; }
    public string ProductName { get; set; }
    public GenderType Gender { get; set; }
    public string Condition { get; set; }
    public string Color { get; set; }
    public string Brand { get; set; }
    public SizeType Size { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}
public class CreateConsignDetailOfflineRequest
{
    public Guid MasterItemId { get; set; }
    public string Note { get; set; }
    public decimal ExpectedPrice { get; set; }
    
    public string Condition { get; set; }
    public string Color { get; set; }
    
    public SizeType Size { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}
public class CreateConsignForSaleOfflineRequest
{
    public Guid MasterItemId { get; set; }
    public string Note { get; set; }
    public decimal ExpectedPrice { get; set; }
    public string ProductName { get; set; }
    public string Condition { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}