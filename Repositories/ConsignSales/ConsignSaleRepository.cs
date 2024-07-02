using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Repositories.ConsignSales;

namespace Repositories.ConsignSales
{
    public class ConsignSaleRepository : IConsignSaleRepository
    {
        private readonly GenericDao<ConsignSale> _consignSaleDao;
        private readonly IMapper _mapper;
        public ConsignSaleRepository(GenericDao<ConsignSale> consignSaleDao, IMapper mapper)
        {
            _consignSaleDao = consignSaleDao;
            _mapper = mapper;
        }

        public async Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSale(Guid accountId, ConsignSaleRequest request)
        {
            try
            {
                var query = _consignSaleDao.GetQueryable();
                query = query.Where(c => c.MemberId == accountId);
                if (!string.IsNullOrWhiteSpace(request.ConsignSaleCode))
                    query = query.Where(x => EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
                if (request.Status != null)
                {
                    query = query.Where(f => f.Status == request.Status);
                }

                if (request.ShopId != null)
                {
                    query = query.Where(f => f.ShopId.Equals(request.ShopId));
                }
                query = query.Where(c => c.MemberId == accountId);

                var count = await query.CountAsync();
                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);

                var items = await query
                    .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                    .AsNoTracking().ToListAsync();

                var result = new PaginationResponse<ConsignSaleResponse>
                {
                    Items = items,
                    PageSize = request.PageSize,
                    TotalCount = count,
                    SearchTerm = request.ConsignSaleCode,
                    PageNumber = request.PageNumber,
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ConsignSaleResponse> GetConsignSaleById(Guid accountId, Guid consignId)
        {
            try
            {
                var consignSale = await _consignSaleDao.GetQueryable().Where(c => c.MemberId == accountId && c.ConsignSaleId == consignId)
                    .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                return consignSale;

            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
