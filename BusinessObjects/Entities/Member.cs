﻿namespace BusinessObjects.Entities;

public class Member : Account
{
    
    public ICollection<ConsignSale> Requests = new List<ConsignSale>();
    public ICollection<Address> Deliveries = new List<Address>();
    public ICollection<Order> Orders = new List<Order>();
    public ICollection<Bid> Bids = new List<Bid>();
    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();
    public ICollection<Refund> Refunds = new List<Refund>();
    public ICollection<Feedback> Feedbacks = new List<Feedback>();
}