using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public class Account
{
    public Guid AccountID { get; set; }

    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
    public string Fullname { get; set; }
    public string Role { get; set; }
}
