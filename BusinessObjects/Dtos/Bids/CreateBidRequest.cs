﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Bids;

public class CreateBidRequest
{
    public Guid MemberId { get; set; }
    [Range(0,int.MaxValue,ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }
}