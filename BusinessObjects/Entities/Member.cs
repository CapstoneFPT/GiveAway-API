using System.Collections;

namespace BusinessObjects.Entities;

public class Member : Account
{
    public ICollection<ConsignSale> Requests = new List<ConsignSale>();
    public ICollection<Address> Deliveries = new List<Address>();
    public ICollection<Order> Orders = new List<Order>();
    public ICollection<Bid> Bids = new List<Bid>();
    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();
    public ICollection<BankAccount> BankAccounts = new List<BankAccount>();
    public ICollection<Feedback> Feedbacks = new List<Feedback>();
    
    public ICollection<Recharge> Recharges = new List<Recharge>();
    public ICollection<Address> Addresses = new List<Address>();
}