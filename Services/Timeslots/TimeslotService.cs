using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Timeslots;
using BusinessObjects.Entities;
using LinqKit;
using Repositories.Timeslots;

namespace Services.Timeslots
{
    public class TimeslotService : ITimeslotService
    {
        private readonly ITimeslotRepository _timeslotRepository;

        public TimeslotService(ITimeslotRepository timeslotRepository)
        {
            _timeslotRepository = timeslotRepository;
        }

        public async Task<PaginationResponse<TimeslotListResponse>> GetTimeslotList(GetTimeslotsRequest request)
        {
            Expression<Func<Timeslot, bool>> predicate = timeslot => true;
            
            if (request.Status != null)
            {
                predicate = predicate.And(timeslot => timeslot.Status == request.Status);
            }
            
            Expression<Func<Timeslot, TimeslotListResponse>> selector = timeslot => new TimeslotListResponse()
            {
                TimeslotId = timeslot.TimeslotId,
                StartTime = timeslot.StartTime,
                EndTime = timeslot.EndTime,
                Slot = timeslot.Slot,
                Status = timeslot.Status
            };

            (List<TimeslotListResponse> Items, int Page, int PageSize, int Total) result =
                await _timeslotRepository.GetTimeslotProjections<TimeslotListResponse>(request.PageNumber,
                    request.PageSize, predicate, selector);

            return new PaginationResponse<TimeslotListResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.Total
            };
        }
    }
}