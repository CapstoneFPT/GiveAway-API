namespace BusinessObjects.Dtos.Commons;

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
}
