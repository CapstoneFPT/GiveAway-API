using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class PointPackage
{
    public Guid PointPackageId { get; set; }
    public int Points { get; set; }
    public int Price { get; set; }
    public PointPackageStatus Status { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

