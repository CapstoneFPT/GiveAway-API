using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; }
    public Roles Role { get; set; }
    public Guid Id { get; set; }
    public string Email { get; set; }
    public Guid? ShopId { get; set; }
}
