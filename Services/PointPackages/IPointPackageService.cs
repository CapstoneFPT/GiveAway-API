using BusinessObjects.Entities;

namespace Services.PointPackages;

public interface IPointPackageService
{
    Task<object?> GetList();
    Task<PointPackage?> GetPointPackageDetail(Guid pointPackageId);
    Task AddPointsToBalance(Guid accountId, int amount);
}