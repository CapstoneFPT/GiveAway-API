using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Category;

public class CategoryTreeNode
{
    public Guid CategoryId { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public string Name { get; set; }
    public CategoryStatus Status { get; set; }
    public List<CategoryTreeNode> Children { get; set; } = new List<CategoryTreeNode>(); 
}