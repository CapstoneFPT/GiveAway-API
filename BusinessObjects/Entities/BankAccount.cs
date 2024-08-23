namespace BusinessObjects.Entities;

public class BankAccount
{
    public Guid BankAccountId { get; set; }

    public string? Bank { get; set; }
    public string? BankLogo {get;set;}
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public bool IsDefault { get; set; }
    
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public DateTime CreatedDate { get; set; }
}