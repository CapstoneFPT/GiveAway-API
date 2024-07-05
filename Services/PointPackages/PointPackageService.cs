using BusinessObjects.Entities;
using Repositories.PointPackages;

namespace Services.PointPackages;

public class PointPackageService : IPointPackageService
{
    private IPointPackageRepository _pointPackageRepository;

    public PointPackageService(IPointPackageRepository pointPackageRepository)
    {
        _pointPackageRepository = pointPackageRepository;
    }
    
    public Task<object?> GetList()
    {
        throw new NotImplementedException();
    }

    public Task<PointPackage?> GetPointPackageDetail(Guid pointPackageId)
    {
        throw new NotImplementedException();
    }

    public Task AddPointsToBalance(Guid orderMemberId, int amount)
    {
        throw new NotImplementedException();
    }
}