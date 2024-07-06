using System.Linq.Expressions;
using BusinessObjects.Entities;

namespace Repositories.PointPackages;

public interface IPointPackageRepository
{
    Task<PointPackage?> GetSingle(Expression<Func<PointPackage, bool>> predicate);
    Task AddPointsToBalance(Guid accountId, int amount);
}