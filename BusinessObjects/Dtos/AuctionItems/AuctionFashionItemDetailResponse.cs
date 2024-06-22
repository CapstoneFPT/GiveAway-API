using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;

namespace BusinessObjects.Dtos.AuctionItems;

public class AuctionFashionItemDetailResponse
{
    public Guid ItemId { get; set; }
    public FashionItemType Type { get; set; }
    public decimal SellingPrice { get; set; }
    public string Name { get; set; }
    public string Note { get; set; }
    public int Quantity { get; set; }
    public decimal? Value { get; set; }
    public string Condition { get; set; }
    public int Duration { get; set; }
    public decimal InitialPrice { get; set; }
    public FashionItemStatus AuctionItemStatus { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public ShopDetailResponse? Shop { get; set; }
}