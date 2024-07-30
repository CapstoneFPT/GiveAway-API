using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Account
{
    [Key]
    public Guid AccountId { get; set; }

    [EmailAddress] public string Email { get; set; }
    public string Phone { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string Fullname { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }
    public Roles Role { get; set; }
    public AccountStatus Status { get; set; }
    public int Balance { get; set; }
    public DateTime CreatedDate { get; set; }

}