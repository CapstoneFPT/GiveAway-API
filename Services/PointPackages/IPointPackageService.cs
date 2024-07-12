using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.PointPackages;
using BusinessObjects.Entities;

namespace Services.PointPackages;

public interface IPointPackageService
{
    Task<PaginationResponse<PointPackageListResponse>> GetList(GetPointPackagesRequest request);
    Task<PointPackageDetailResponse?> GetPointPackageDetail(Guid pointPackageId);
    Task AddPointsToBalance(Guid accountId, int amount);
}