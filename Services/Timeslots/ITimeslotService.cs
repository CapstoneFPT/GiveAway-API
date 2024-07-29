using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Timeslots;

namespace Services.Timeslots
{
    public interface ITimeslotService
    {
        Task<PaginationResponse<TimeslotListResponse>> GetTimeslotList(GetTimeslotsRequest request);
    }
}
