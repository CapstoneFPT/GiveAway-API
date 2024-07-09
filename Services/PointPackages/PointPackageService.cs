using System.Linq.Expressions;
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

    public async Task<PointPackage?> GetPointPackageDetail(Guid pointPackageId)
    {
        try
        {
            var result = await _pointPackageRepository.GetSingle(x => x.PointPackageId == pointPackageId);

            return result;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public Task AddPointsToBalance(Guid accountId, int amount)
    {
        try
        {
            
            return _pointPackageRepository.AddPointsToBalance(accountId, amount);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }
}