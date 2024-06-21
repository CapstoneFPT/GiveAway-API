using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Category;

public class CategoryRequest
{
    [Required]
    public string Name { get; set; }
}