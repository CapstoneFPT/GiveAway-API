using System.Linq.Expressions;
using BusinessObjects.Entities;
using Repositories.PointPackages;

namespace Services.PointPackages;

public class PointPackageService : IPointPackageService
{
    private readonly IPointPackageRepository _pointPackageRepository;

    public PointPackageService(IPointPackageRepository pointPackageRepository)
    {
        _pointPackageRepository = pointPackageRepository;
    }

    public Task<object?> GetList()
    {
        throw new NotImplementedException();
    }

    public async Task<PointPackage?> GetPointPackageDetail(Guid pointPackageId)
    {
        var result = await _pointPackageRepository.GetSingle(x => x.PointPackageId == pointPackageId);

        return result;
    }

    public Task AddPointsToBalance(Guid accountId, int amount)
    {
        return _pointPackageRepository.AddPointsToBalance(accountId, amount);
    }
}