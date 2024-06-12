using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Auctions;

public class CreateAuctionRequest : IValidatableObject
{
    public string Title { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public Guid TimeslotId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Error: Minimum deposit must be greater than 0")]
    public int DepositFee { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOnly.FromDateTime(StartDate) < DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult("Start date must be greater than current date",
                new[] { nameof(StartDate) });
        }

        if (StartDate > EndDate)
        {
            yield return new ValidationResult("Start date must be less than end date", new[] { nameof(StartDate) });
        }

        if (ScheduleDate < DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult("Schedule date must be greater than current date",
                new[] { nameof(ScheduleDate) });
        }
    }
}