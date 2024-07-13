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
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Refunds
{
    public class RefundRepository : IRefundRepository
    {
        private readonly GenericDao<Refund> _refundDao;
        private readonly GenericDao<OrderDetail> _orderDetailDao;
        private readonly GenericDao<FashionItem> _fashionitemDao;
        private readonly IMapper _mapper;

        public RefundRepository(GenericDao<Refund> refundDao, GenericDao<OrderDetail> orderDetailDao, 
            GenericDao<FashionItem> fashionitemDao, IMapper mapper)
        {
            _refundDao = refundDao;
            _orderDetailDao = orderDetailDao;
            _fashionitemDao = fashionitemDao;
            _mapper = mapper;
        }

        public async Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, RefundStatus refundStatus)
        {
            var refund = await _refundDao.GetQueryable().Include(c => c.OrderDetail)
                .ThenInclude(c => c.FashionItem)
                .FirstOrDefaultAsync(c => c.RefundId == refundId);
            if (refundStatus.Equals(RefundStatus.Approved))
            {
                refund.RefundStatus = RefundStatus.Approved;
            }
            if (refundStatus.Equals(RefundStatus.Rejected))
            {
                refund.RefundStatus = RefundStatus.Rejected;        
            }

            await _refundDao.UpdateAsync(refund);
            return await GetRefundById(refundId);
        }

        public async Task<RefundResponse> GetRefundById(Guid refundId)
        {
            var refund = await _refundDao.GetQueryable().Include(c => c.OrderDetail)
                .Where(c => c.RefundId == refundId)
                .Select(c => new RefundResponse()
                {
                    RefundId = c.RefundId,
                    CreatedDate = c.CreatedDate,
                    Description = c.Description,
                    MemberId = c.OrderDetail.Order.MemberId,
                    Member = c.OrderDetail.Order.Member,
                    OrderDetailId = c.OrderDetailId,
                    RefundStatus = c.RefundStatus,
                }).FirstOrDefaultAsync();

            var orderDetail = await _orderDetailDao.GetQueryable()
                .Include(c => c.FashionItem)
                .FirstOrDefaultAsync(c => c.OrderDetailId == refund.OrderDetailId);
            var orderDetailResponse = new OrderDetailResponse<FashionItemDetailResponse>()
            {
                OrderId = orderDetail.OrderId,
                RefundExpirationDate = orderDetail.RefundExpirationDate,
                UnitPrice = orderDetail.UnitPrice,
                FashionItemDetail = _mapper.Map<FashionItemDetailResponse>(orderDetail.FashionItem)
            };
            refund.FashionItem = orderDetailResponse;
            return refund;
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
