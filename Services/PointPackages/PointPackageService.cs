using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.PointPackages;
using BusinessObjects.Entities;
using LinqKit;
using Repositories.PointPackages;

namespace Services.PointPackages;

public class PointPackageService : IPointPackageService
{
    private readonly IPointPackageRepository _pointPackageRepository;

    public PointPackageService(IPointPackageRepository pointPackageRepository)
    {
        _pointPackageRepository = pointPackageRepository;
    }


    public async Task<PaginationResponse<PointPackageListResponse>> GetList(GetPointPackagesRequest request)
    {
        Expression<Func<PointPackage, bool>> predicate = package => true;
        
        if(request.Status.Length > 0)
        {
            predicate = predicate.And(x => request.Status.Contains(x.Status));
        }
        
        Expression<Func<PointPackage, PointPackageListResponse>> selector = package => new PointPackageListResponse()
        {
            PointPackageId = package.PointPackageId,
            Points = package.Points, Status = package.Status,
            Price = package.Price
        };
        
       (List<PointPackageListResponse> Items, int Page, int PageSize, int TotalCount) result = await _pointPackageRepository.GetPointPackages<PointPackageListResponse>(request.Page, request.PageSize, predicate, selector);
       
        return new PaginationResponse<PointPackageListResponse>()
        {
            Items = result.Items,
            PageNumber = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<PointPackageDetailResponse?> GetPointPackageDetail(Guid pointPackageId)
    {
        var result = await _pointPackageRepository.GetSingle<PointPackageDetailResponse>(x => x.PointPackageId == pointPackageId, x => new PointPackageDetailResponse()
        {
            PointPackageId = x.PointPackageId,
            Points = x.Points,
            Status = x.Status,
            Price = x.Price
        });

        return result;
    }

    public Task AddPointsToBalance(Guid accountId, int amount)
    {
        return _pointPackageRepository.AddPointsToBalance(accountId, amount);
    }
}