using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Timeslots;

public class CreateTimeslotRequest
{
    [MaxLength(3, ErrorMessage = "Cannot create more than 3 timeslots at a time")]
    public CreateTimeslotData[] Data { get; set; } = [];
}

public class CreateTimeslotData
{
    [Required] public TimeOnly StartTime { get; set; }
    [Required] public TimeOnly EndTime { get; set; }
    [Required] public int Slot { get; set; }
}