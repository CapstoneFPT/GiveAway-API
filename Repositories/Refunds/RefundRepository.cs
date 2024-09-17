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
        private const string Prefix = "REF";
        private static Random _random = new();

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
                throw new RefundNotFoundException();
            }
            if (request.Status.Equals(RefundStatus.Approved))
            {
                refund.RefundStatus = RefundStatus.Approved;
                
                refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.AwaitingReturn;
            }
            else if (request.Status.Equals(RefundStatus.Rejected))
            {
                refund.RefundStatus = RefundStatus.Rejected;
                
                refund.ResponseFromShop = request.ResponseFromShop;
                refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Sold;
            }
            else
            {
                throw new StatusNotAvailableException();
            }
            
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
            return new RefundResponse()
            {
                RefundId = refund.RefundId,
                RefundStatus = refund.RefundStatus,
                RefundPercentage = refund.RefundPercentage,
                ResponseFromShop = refund.ResponseFromShop,
                ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode
            };
        }
        public async Task<string> GenerateUniqueString()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string code = GenerateCode();
                bool isCodeExisted = await _giveAwayDbContext.Recharges.AnyAsync(r => r.RechargeCode == code);

                if (!isCodeExisted)
                {
                    return code;
                }

                await Task.Delay(100 * (int)Math.Pow(2, attempt));
            }

            throw new Exception("Failed to generate unique code after multiple attempts");
        }

        private static string GenerateCode()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string randomString = _random.Next(1000, 9999).ToString();
            return $"{Prefix}-{timestamp}-{randomString}";
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

        public async Task<Refund?> GetSingleRefund(Expression<Func<Refund, bool>> predicate)
        {
            return await GenericDao<Refund>.Instance.GetQueryable()
                .Include(c => c.OrderLineItem)
                .ThenInclude(c => c.Order)
                .Include(c => c.Images)
                .Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem)
                .ThenInclude(c => c.MasterItem)
                .Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem)
                .ThenInclude(ind => ind.Images)
                .Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem).ThenInclude(c => c.ConsignSaleLineItem)
                .Where(predicate)
                .FirstOrDefaultAsync();
        }


        public async Task<RefundResponse> ConfirmReceivedAndRefund(Guid refundId, ConfirmReceivedRequest request)
        {
            var refund = await _giveAwayDbContext.Refunds.AsQueryable().Include(c => c.OrderLineItem)
                .ThenInclude(c => c.IndividualFashionItem).Where(c => c.RefundId == refundId).FirstOrDefaultAsync();
            if (refund is null)
            {
                throw new RefundNotFoundException();
            }
            refund.RefundStatus = RefundStatus.Completed;
            refund.OrderLineItem.IndividualFashionItem.Status = FashionItemStatus.Returned;
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
            return new RefundResponse()
            {
                RefundId = refund.RefundId,
                RefundStatus = refund.RefundStatus
            };
        }

        public async Task CreateRefund(Refund refund)
        {
            refund.RefundCode = await GenerateUniqueString();
            await GenericDao<Refund>.Instance.AddAsync(refund);
        }

        public IQueryable<Refund> GetQueryable()
        {
            return _giveAwayDbContext.Refunds.AsQueryable();
        }
    
        public async Task UpdateRefund(Refund refund)
        {
            await GenericDao<Refund>.Instance.UpdateAsync(refund);
        }
    }
}
