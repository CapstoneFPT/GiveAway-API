using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;

namespace BusinessObjects.Dtos.Auctions;

public class AuctionDetailResponse
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DepositFee { get; set; }
    public AuctionStatus Status { get; set; }
    public decimal StepIncrement { get; set; }
    public string AuctionCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public string IndividualItemCode { get; set; }
    public string ShopAddress { get; set; }
    public bool Won { get; set; }
}

public class AuctionItemDetailResponse
{
    public Guid ItemId { get; set; }
    public FashionItemType FashionItemType { get; set; }
    public decimal SellingPrice { get; set; } 
    public string Name { get; set; }
    public string Note { get; set; }
    public FashionItemStatus Status { get; set; }
    public string Condition { get; set; }
    public string Brand { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public GenderType Gender { get; set; }
    public string Description { get; set; }
    public decimal? InitialPrice { get; set; }
    public List<string> Images { get; set; } = [];
    public string CategoryName { get; set; }
    public string ShopAddress { get; set; }
    public string ItemCode { get; set; }
    public Guid AuctionId { get; set; }
}

public class AuctionItemCategory
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int Level { get; set; }
}

public class FashionItemImage
{
    public Guid ImageId { get; set; }
    public string ImageUrl { get; set; }
}

public class ShopAuctionDetailResponse
{
    public Guid ShopId { get; set; }
    public string Address { get; set; }
}