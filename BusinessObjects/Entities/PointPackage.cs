using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class PointPackage
{
    public Guid PointPackageId { get; set; }
    public int Points { get; set; }
    public int Price { get; set; }
    public PointPackageStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<OrderLineItem> OrderLineItems { get; set; } = new List<OrderLineItem>();
}

