using System.ComponentModel.DataAnnotations;

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
    public string Role { get; set; }
    public string Status { get; set; }

    public ICollection<Request> Requests = new List<Request>();
    public ICollection<Delivery> Deliveries = new List<Delivery>();
    public ICollection<Order> Orders = new List<Order>();
    public ICollection<Bid> Bids = new List<Bid>();
    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();

    public Wallet Wallet { get; set; }
    public Shop Shop { get; set; }
}