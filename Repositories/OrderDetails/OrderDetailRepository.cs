using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Entities;
using Dao;

namespace Repositories.OrderDetails
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly GenericDao<OrderDetail> _orderdetailDao;

        public OrderDetailRepository(GenericDao<OrderDetail> orderdetailDao)
        {
            _orderdetailDao = orderdetailDao;
        }

        public Task<PaginationResponse<OrderDetailResponse>> GetAllOrderDetailByOrderId(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
