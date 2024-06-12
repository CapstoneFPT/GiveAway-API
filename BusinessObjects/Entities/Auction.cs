﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Auction
{
    [Key]
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DepositFee { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public AuctionFashionItem AuctionFashionItem { get; set; }
    public Guid AuctionItemId { get; set; }
    public string Status { get; set; }

    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();
    public ICollection<Bid> Bids = new List<Bid>();
    
    public Schedule Schedule { get; set; }
}