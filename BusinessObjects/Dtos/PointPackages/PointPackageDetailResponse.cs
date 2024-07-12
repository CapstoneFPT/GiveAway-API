using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.PointPackages;

public class PointPackageDetailResponse
{
    public Guid PointPackageId { get; set; }
    public int Points { get; set; }
    public int Price { get; set; }
    public PointPackageStatus Status { get; set; }
}