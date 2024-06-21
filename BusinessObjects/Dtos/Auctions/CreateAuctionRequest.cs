using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Auctions;

public class CreateAuctionRequest : IValidatableObject
{
    public string Title { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public Guid TimeslotId { get; set; }
    [Range(1,100,ErrorMessage = "Value must be between 1 and 100")]
    public int StepIncrementPercentage { get; set; }
    

    [Range(1, int.MaxValue, ErrorMessage = "Error: Minimum deposit must be greater than 0")]
    public int DepositFee { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (ScheduleDate < DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult("Schedule date must be greater than current date",
                new[] { nameof(ScheduleDate) });
        }
    }
}