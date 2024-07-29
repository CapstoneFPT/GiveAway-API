using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Timeslots;

public class TimeslotListResponse
{
    public Guid TimeslotId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Slot { get; set; }
    public TimeSlotStatus Status { get; set; }
}