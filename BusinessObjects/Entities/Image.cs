using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Image
{
    [Key]
    public Guid ImageId { get; set; }
    public string Url { get; set; }
    public FashionItem FashionItem { get; set; }
    public Guid ItemId { get; set; }
}