namespace BusinessObjects.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; }
    public string Role { get; set; }
    public Guid Id { get; set; }
    public string Email { get; set; }
}
