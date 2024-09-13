using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.ConsignSaleLineItems;

public class ConsignSaleLineItemRepository : IConsignSaleLineItemRepository
{
    private readonly IMapper _mapper;

    public ConsignSaleLineItemRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<List<ConsignSaleLineItemsListResponse>> GetConsignSaleLineItemsByConsignSaleId(Guid consignSaleId)
    {
        var lstconsignSaleDetail = await GenericDao<ConsignSaleLineItem>.Instance.GetQueryable()
            .Where(c => c.ConsignSaleId == consignSaleId)
            .ProjectTo<ConsignSaleLineItemsListResponse>(_mapper.ConfigurationProvider).AsNoTracking().ToListAsync();
        if (lstconsignSaleDetail.Count == 0)
        {
            throw new ConsignSaleLineItemNotFoundException();
        }

        return lstconsignSaleDetail;
    }

    public IQueryable<ConsignSaleLineItem> GetQueryable()
    {
       return GenericDao<ConsignSaleLineItem>.Instance.GetQueryable(); 
    }

    public async Task AddConsignSaleLineItem(ConsignSaleLineItem consignSaleLineItem)
    {
        await GenericDao<ConsignSaleLineItem>.Instance.AddAsync(consignSaleLineItem);
    }

    public async Task<ConsignSaleLineItem?> GetSingleConsignSaleLineItem(Expression<Func<ConsignSaleLineItem, bool>> predicate)
    {
        var result = await GenericDao<ConsignSaleLineItem>.Instance
            .GetQueryable()
            .Include(c => c.Images)
            .Include(c => c.ConsignSale)
            .ThenInclude(c => c.Shop)
            .SingleOrDefaultAsync(predicate);
        return result;
    }

    public async Task UpdateConsignLineItem(ConsignSaleLineItem consignSaleLineItem)
    {
        await GenericDao<ConsignSaleLineItem>.Instance.UpdateAsync(consignSaleLineItem);
    }
}