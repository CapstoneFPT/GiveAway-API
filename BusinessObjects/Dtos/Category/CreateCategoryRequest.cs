using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Category;

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; }
}