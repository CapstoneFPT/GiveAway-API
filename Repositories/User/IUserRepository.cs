namespace Repositories.User;

public interface IUserRepository
{
    Task<List<BusinessObjects.User>> FindMany(
        Func<BusinessObjects.User, bool> predicate,
        int page,
        int pageSize
    );
    Task<BusinessObjects.User?> FindOne(Func<BusinessObjects.User, bool> predicate);
    Task ResetPassword(Guid uid, string password);
    Task<BusinessObjects.User> FindUserByEmail(string email);
    Task<BusinessObjects.User> ResetPasswordToken(BusinessObjects.User user);
    Task<BusinessObjects.User> FindUserByPasswordResetToken(string token);
}
