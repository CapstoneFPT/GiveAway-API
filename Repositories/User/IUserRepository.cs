namespace Repositories.User;

public interface IUserRepository
{
    Task<List<BusinessObjects.User>> FindMany(
        Func<BusinessObjects.User, bool> predicate,
        int page,
        int pageSize
    );
    Task<BusinessObjects.User?> FindOne(Func<BusinessObjects.User, bool> predicate);
}
