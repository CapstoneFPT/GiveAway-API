using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Category
{
    [Key]
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public Category? Parent { get; set; }
    public Guid? ParentId { get; set; }
    
    public ICollection<Category> Children = new List<Category>();
    public ICollection<FashionItem> FashionItems { get; set; }
}