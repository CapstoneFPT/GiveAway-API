using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public  class FashionItem
{
    [Key]
    public Guid ItemId { get; set; }
    public string Type { get; set; }
    public int SellingPrice { get; set; }
    public string Name { get; set; }
    public string Note { get; set; }
    public int Quantity { get; set; }
    public int? Value { get; set; }
    
    public string Condition { get; set; }
    public Request Request { get; set; }
    public Guid RequestId { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public Category Category { get; set; }
    public Guid CategoryId { get; set; }
    public string Status { get; set; }
}