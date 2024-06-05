namespace BusinessObjects.Entities;

public class PointPackage
{
    public Guid PointPackageId { get; set; }
    public int Points { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
}