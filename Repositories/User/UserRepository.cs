using BusinessObjects;

namespace Repositories.User;

public class UserRepository : IUserRepository
{
    public static List<BusinessObjects.User> Users = new List<BusinessObjects.User>()
    {
        new BusinessObjects.User()
        {
            Id = new Guid(),
            Email = "admin@gmail.com",
            Password = "password123",
            Role = Role.Admin
        },
        new BusinessObjects.User()
        {
            Id = new Guid(),
            Email = "staff@gmail.com",
            Password = "password123",
            Role = Role.Staff
        },
        new BusinessObjects.User()
        {
            Id = new Guid(),
            Email = "user@gmail.com",
            Password = "password123",
            Role = Role.User
        }
    };

    public Task<List<BusinessObjects.User>> FindMany(
        Func<BusinessObjects.User, bool> predicate,
        int page,
        int pageSize
    )
    {
        try
        {
            var users = Users.Where(predicate).Skip((page * pageSize) - 1).Take(pageSize).ToList();

            return Task.FromResult(users);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public Task<BusinessObjects.User?> FindOne(Func<BusinessObjects.User, bool> predicate)
    {
        try
        {
            var result = Users.FirstOrDefault(predicate);
            return Task.FromResult<BusinessObjects.User?>(result);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
