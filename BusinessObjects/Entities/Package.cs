namespace BusinessObjects.Entities;

public class Package
{
    public Guid PackageId { get; set; }
    public int Points { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
}