namespace BusinessObjects;

public enum Role
{
   User,
   Staff,
   Admin
}

public class User
{
   public Guid Id { get; set; }
   public string Email { get; set; } 
   public string Password { get; set; }
   public Role Role { get; set; }
}