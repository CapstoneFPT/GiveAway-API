using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.ConsignSaleDetails;

public class ConsignSaleDetailRepository : IConsignSaleDetailRepository
{
    private readonly IMapper _mapper;

    public ConsignSaleDetailRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<List<ConsignSaleDetailResponse>> GetConsignSaleDetailsByConsignSaleId(Guid consignSaleId)
    {
        var lstconsignSaleDetail = await GenericDao<ConsignSaleDetail>.Instance.GetQueryable()
            .Where(c => c.ConsignSaleId == consignSaleId)
            .ProjectTo<ConsignSaleDetailResponse>(_mapper.ConfigurationProvider).AsNoTracking().ToListAsync();
        if (lstconsignSaleDetail.Count == 0)
        {
            throw new ConsignSaleDetailsNotFoundException();
        }

        return lstconsignSaleDetail;
    }
}