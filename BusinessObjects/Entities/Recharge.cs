using System;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Recharge
{
  public Guid RechargeId { get; set; }
  public decimal Amount { get; set; }
  public string RechargeCode { get; set; } 
  public DateTime CreatedDate { get; set; }
  public RechargeStatus Status { get; set; }
  public PaymentMethod PaymentMethod { get; set; }
  public Guid MemberId { get; set; }
  public Member Member { get; set; } = default!;
  public Transaction? Transaction { get; set; } = default!;
}
