using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.OrderDetails;

namespace Repositories.OrderDetails
{
    public interface IOrderDetailRepository
    {
        Task<PaginationResponse<OrderDetailResponse>> GetAllOrderDetailByOrderId(Guid id);
    }
}
