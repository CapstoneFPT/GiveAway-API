using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Timeslots;

public class GetTimeslotsRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public TimeSlotStatus? Status { get; set; }
}