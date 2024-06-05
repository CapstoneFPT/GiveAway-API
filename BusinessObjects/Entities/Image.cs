using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Image
{
    [Key]
    public Guid ImageId { get; set; }
    public string Url { get; set; }
    public Item Item { get; set; }
    public Guid ItemId { get; set; }
}