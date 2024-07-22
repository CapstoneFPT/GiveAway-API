using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class FashionItem
{
    [Key] public Guid ItemId { get; set; }
    public FashionItemType Type { get; set; }
    public int SellingPrice { get; set; }
    public string Name { get; set; }
    public string Note { get; set; }
    public int? Value { get; set; }

    public string Condition { get; set; }
    public ConsignSaleDetail ConsignSaleDetail { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public Category? Category { get; set; }
    public Guid? CategoryId { get; set; }
    public FashionItemStatus Status { get; set; }
    public SizeType Size { get; set; }
    public string Color { get; set; }
    public string? Brand { get; set; } = "No Brand";
    public GenderType Gender { get; set; }
}



