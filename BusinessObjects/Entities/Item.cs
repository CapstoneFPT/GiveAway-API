using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public  class Item
{
    [Key]
    public Guid ItemId { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }
    public string Name { get; set; }
    public string Note { get; set; }
    public decimal Value { get; set; }
    public int Condition { get; set; }
    public Request Request { get; set; }
    public Guid RequestId { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public Category Category { get; set; }
    public Guid CategoryId { get; set; }
    public string Status { get; set; }
}