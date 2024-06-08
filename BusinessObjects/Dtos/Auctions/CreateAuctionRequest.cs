using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Auctions;

public class CreateAuctionRequest : ValidationAttribute
{
    public string Title { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public Guid TimeslotId { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return base.IsValid(value, validationContext);
    }
}