using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
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
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public RefundRepository(IMapper mapper, GiveAwayDbContext giveAwayDbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = giveAwayDbContext;
        }

        public async Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, ApprovalRefundRequest request)
        {
            var refund = await GenericDao<Refund>.Instance.GetQueryable().Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem)
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
                refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Returned;
            }
            else if (request.Status.Equals(RefundStatus.Rejected))
            {
                refund.RefundStatus = RefundStatus.Rejected;
                refund.RefundPercentage = 0;
                refund.ResponseFromShop = request.Description;
                refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Sold;
            }
            else
            {
                throw new StatusNotAvailableException();
            }
            
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
            var response = await GetRefundById(refundId);
            
            return response;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetRefundProjections<T>(int? page, int? pageSize, Expression<Func<Refund, bool>>? predicate, Expression<Func<Refund, T>>? selector)
        {
            var query = _giveAwayDbContext.Refunds.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate).OrderByDescending(c => c.CreatedDate);
            }

            var count = await query.CountAsync();

            var pageNum = page ?? -1;
            var pageSizeNum = pageSize ?? -1;

            if (pageNum > 0 && pageSizeNum > 0)
            {
                query = query.Skip((pageNum - 1) * pageSizeNum).Take(pageSizeNum);
            }

            List<T> items = new List<T>();
            if (selector != null)
            {
                items = await query.Select(selector).ToListAsync();
            }
            else
            {
                items = await query.Cast<T>().ToListAsync();
            }

            return (items, pageNum, pageSizeNum, count);
        }

        public async Task<RefundResponse> GetRefundById(Guid refundId)
        {
            var refund = await GenericDao<Refund>.Instance.GetQueryable()
                .Include(c => c.OrderLineItem).ThenInclude(c => c.Order)
                .Where(c => c.RefundId == refundId)
                .ProjectTo<RefundResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            
            return refund;
        }


        public async Task<RefundResponse> ConfirmReceivedAndRefund(Guid refundId)
        {
            var refund = await _giveAwayDbContext.Refunds.AsQueryable().Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem).Where(c => c.RefundId == refundId).FirstOrDefaultAsync();
            refund.RefundStatus = RefundStatus.Completed;
            refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Unavailable;
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
            return await GetRefundById(refundId);
        }

        public async Task CreateRefund(Refund refund)
        {
            await GenericDao<Refund>.Instance.AddAsync(refund);
        }
    }
}
