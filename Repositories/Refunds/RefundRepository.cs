using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

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

        public async Task<PaginationResponse<RefundResponse>> GetRefundsByShopId(Guid shopId, RefundRequest request)
        {
            var query = _refundDao.GetQueryable();
            if (request.Status != null)
            {
                query = query.Where(f => request.Status.Contains(f.RefundStatus));
            }

            if (request.PreviousTime != null)
            {
                query = query.Where(f => f.CreatedDate <= request.PreviousTime);
            }

            query = query.OrderBy(c => c.CreatedDate);
            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                .ProjectTo<RefundResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<RefundResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                Filters = ["Filter created date by ascending"],
                PageNumber = request.PageNumber,
            };
            return result;
        }
    }
}
