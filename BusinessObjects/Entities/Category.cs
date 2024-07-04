using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Category
{
    [Key]
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public Category? Parent { get; set; }

    public ICollection<Category> Children = new List<Category>();
    public ICollection<FashionItem> FashionItems { get; set; }
    public CategoryStatus Status { get; set; }
}