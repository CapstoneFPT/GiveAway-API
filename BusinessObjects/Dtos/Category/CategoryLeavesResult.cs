namespace BusinessObjects.Dtos.Category;

public class CategoryLeavesResponse
{
    public Guid? ShopId { get; set; }
    public List<CategoryTreeNode> CategoryLeaves { get; set; } = [];
}