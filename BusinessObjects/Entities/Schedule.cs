using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Schedule
{
   [Key]
   public Guid ScheduleId { get; set; }

   public DateOnly Date { get; set; }
   public Timeslot Timeslot { get; set; }
   public Guid TimeslotId { get; set; }
   public Auction Auction { get; set; }
   public Guid AuctionId { get; set; }
}