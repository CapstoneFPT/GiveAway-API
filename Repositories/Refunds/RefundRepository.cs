using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Refunds
{
    public class RefundRepository : IRefundRepository
    {
      
        private readonly IMapper _mapper;

        public RefundRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, ApprovalRefundRequest request)
        {
            var refund = await GenericDao<Refund>.Instance.GetQueryable().Include(c => c.OrderDetail)
                .ThenInclude(c => c.FashionItem)
                .FirstOrDefaultAsync(c => c.RefundId == refundId);
            if (refund == null)
            {
                throw new RefundNoFoundException();
            }
            if (request.Status.Equals(RefundStatus.Approved))
            {
                refund.RefundStatus = RefundStatus.Approved;
                refund.RefundPercentage = request.RefundPercentage;
                refund.ResponseFromShop = request.Description;
                refund.OrderDetail.FashionItem.Status = FashionItemStatus.Returned;
            }
            if (request.Status.Equals(RefundStatus.Rejected))
            {
                refund.RefundStatus = RefundStatus.Rejected;
                refund.RefundPercentage = 0;
                refund.ResponseFromShop = request.Description;
            }
            
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
            var response = await GetRefundById(refundId);
            
            return response;
        }

        public async Task<RefundResponse> GetRefundById(Guid refundId)
        {
            var refund = await GenericDao<Refund>.Instance.GetQueryable()
                .Include(c => c.OrderDetail).ThenInclude(c => c.Order)
                .Where(c => c.RefundId == refundId)
                .ProjectTo<RefundResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            
            return refund;
        }

        public async Task<PaginationResponse<RefundResponse>> GetAllRefunds(RefundRequest request)
        {
            var query = GenericDao<Refund>.Instance.GetQueryable();
            if (request.Status != null)
            {
                query = query.Where(f => request.Status.Contains(f.RefundStatus));
            }

            if (request.PreviousTime != null)
            {
                query = query.Where(f => f.CreatedDate <= request.PreviousTime);
            }

            if (request.ShopId != null)
            {
                query = query.Where(c => c.OrderDetail.FashionItem.ShopId == request.ShopId);
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
                /*Filters = ["Filter created date by ascending"],*/
                PageNumber = request.PageNumber,
            };
            return result;
        }
    }
}
