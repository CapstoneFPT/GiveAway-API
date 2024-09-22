using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class MasterFashionItem
{
    public Guid MasterItemId { get; set; }
    public string MasterItemCode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public GenderType Gender { get; set; }
    public Category Category { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsConsignment { get; set; }
    public ICollection<Image> Images { get; set; } = [];
    
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
   
    public ICollection<IndividualFashionItem> IndividualFashionItems { get; set; } = [];
}

public class IndividualFashionItem
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; }
    public Guid MasterItemId { get; set; }
    public string? Note { get; set; }
    public decimal? SellingPrice { get; set; }
    public FashionItemStatus Status { get; set; }
    
    public string Color { get; set; } = "N/A";
    public SizeType Size { get; set; }
    public string Condition { get; set; }
    public FashionItemType Type { get; set; }
    public MasterFashionItem MasterItem { get; set; }
    public ConsignSaleLineItem? ConsignSaleLineItem { get; set; }
    public Guid? ConsignSaleLineItemId { get; set; }
    public DateTime CreatedDate { get; set; }
    public ICollection<Image> Images { get; set; } = [];
}

public class IndividualCustomerSaleFashionItem : IndividualFashionItem
{
    
}

public class IndividualConsignedForSaleFashionItem : IndividualFashionItem
{
}

public class IndividualAuctionFashionItem : IndividualFashionItem
{
    public decimal? InitialPrice { get; set; }
    public ICollection<Auction> Auctions { get; set; } = [];
}