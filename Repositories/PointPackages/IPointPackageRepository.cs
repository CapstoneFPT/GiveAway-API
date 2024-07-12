using System.Linq.Expressions;
using BusinessObjects.Dtos.PointPackages;
using BusinessObjects.Entities;

namespace Repositories.PointPackages;

public interface IPointPackageRepository
{
    Task<PointPackage?> GetSingle(Expression<Func<PointPackage, bool>> predicate);
    Task AddPointsToBalance(Guid accountId, int amount);
    Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetPointPackages<T>(int page,
        int pageSize, Expression<Func<PointPackage, bool>> predicate,
        Expression<Func<PointPackage, T>> selector);
}