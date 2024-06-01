using BusinessObjects;
using System.Security.Cryptography;

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
            Email = "thosanquy666@gmail.com",
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

    public Task<BusinessObjects.User> FindUserByEmail(string email)
    {
        var user = Users.FirstOrDefault( c => c.Email == email);
        return Task.FromResult((user == null) ? null : user);
    }

    public Task<BusinessObjects.User> FindUserByPasswordResetToken(string token)
    {
        var user = Users.FirstOrDefault(c => c.PasswordResetToken == token);
        return Task.FromResult((user == null) ? null : user);
    }

    public Task ResetPassword(Guid uid, string password)
    {
        try
        {
            var user = Users.FirstOrDefault(c => c.Id == uid);
            if (user == null)
            {
                throw new Exception();
            }
            else
            {
                user.Password = password;
                return Task.FromResult(user);
            }
        }catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<BusinessObjects.User> ResetPasswordToken(BusinessObjects.User user)
    {
        user.PasswordResetToken = CreateRandomToken();
        user.ResetTokenExpires = DateTime.Now.AddDays(1);
        return await Task.FromResult(user);
    }
    private string CreateRandomToken()
    {
        Random random = new Random();

        // Tạo một số ngẫu nhiên gồm 6 chữ số
        int randomNumber = random.Next(100000, 999999);
        return randomNumber.ToString();
    }
}
