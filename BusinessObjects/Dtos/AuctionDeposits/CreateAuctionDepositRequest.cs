using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.AuctionDeposits;

public class CreateAuctionDepositRequest
{
    [Required]
    public Guid MemberId { get; set; }
}