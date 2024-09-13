using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Auctions;

public class CreateAuctionRequest : IValidatableObject
{
    [Required]
    public string Title { get; set; }
    [Required]
    public Guid ShopId { get; set; }
    [Required]
    public Guid AuctionItemId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    
    [Range(1,100,ErrorMessage = "Value must be between 1 and 100")]
    public decimal StepIncrementPercentage { get; set; }
    

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Error: Minimum deposit must be greater than 0")]
    public decimal DepositFee { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.ToUniversalTime() < DateTime.UtcNow)
        {
            yield return new ValidationResult("Start time must be greater than current time",
                new[] { nameof(StartTime) });
        }
        
        if (EndTime.ToUniversalTime() < DateTime.UtcNow)
        {
            yield return new ValidationResult("End time must be greater than current time",
                new[] { nameof(EndTime) });
        }
        
        if (StartTime >= EndTime)
        {
            yield return new ValidationResult("Start time must be less than end time",
                new[] { nameof(StartTime), nameof(EndTime) });
        }
    }
}