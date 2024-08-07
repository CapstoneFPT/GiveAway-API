using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class MasterFashionItem
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public GenderType Gender { get; set; }
    public Category Category { get; set; }
    public ICollection<FashionItemVariation> Variations { get; set; } = [];
}

public class IndividualFashionItem
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; }
    public Guid VariationId { get; set; }
    public string Note { get; set; }
    public FashionItemStatus Status { get; set; }
    public Guid ShopId { get; set; }
    public Shop Shop { get; set; }
    public FashionItemType Type { get; set; }
    public FashionItemVariation Variation { get; set; }
    public ConsignSaleDetail ConsignSaleDetail { get; set; }
    public ICollection<Image> Images { get; set; } = [];
}

public class FashionItemVariation
{
    public Guid Id { get; set; }
    public Guid MasterItemId { get; set; }
    public string Condition { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public int StockCount { get; set; }
    public MasterFashionItem MasterItem { get; set; }
    public ICollection<IndividualFashionItem> IndividualItems { get; set; } = [];
}

public class IndividualConsignedForSaleFashionItem : IndividualFashionItem
{
}

public class IndividualAuctionFashionItem : IndividualFashionItem
{
    public decimal? InitialPrice { get; set; }
}