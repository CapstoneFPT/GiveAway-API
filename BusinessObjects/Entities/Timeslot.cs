using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Timeslot

{
    [Key]
    public Guid TimeslotId { get; set; }
    public int Slot { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<Schedule> Schedules = new List<Schedule>();
}