using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Auctions;

public class CreateAuctionRequest 
{
    public string Title { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public Guid TimeslotId { get; set; }
    public int DepositFee { get; set; }

}