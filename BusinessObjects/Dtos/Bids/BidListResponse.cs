﻿namespace BusinessObjects.Dtos.Bids;

public class BidListResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}