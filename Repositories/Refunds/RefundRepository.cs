using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Entities;
using Dao;

namespace Repositories.Refunds
{
    public class RefundRepository : IRefundRepository
    {
        private readonly GenericDao<Refund> _refundDao;
        private readonly IMapper _mapper;

        public RefundRepository(GenericDao<Refund> refundDao, IMapper mapper)
        {
            _refundDao = refundDao;
            _mapper = mapper;
        }
    }
}
