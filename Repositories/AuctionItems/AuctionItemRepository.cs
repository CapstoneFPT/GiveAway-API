using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.AuctionItems
{
    public class AuctionItemRepository : IAuctionItemRepository
    {
        private readonly GenericDao<AuctionFashionItem> _auctionFashionItemDao;

        public AuctionItemRepository(GenericDao<AuctionFashionItem> auctionFashionItemDao)
        {
            _auctionFashionItemDao = auctionFashionItemDao;
        }

        public async Task<AuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId,
            FashionItemStatus fashionItemStatus)
        {
            var auctionItem = await _auctionFashionItemDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == auctionFashionItemId);
            if (auctionItem is null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = fashionItemStatus;
            return await _auctionFashionItemDao.UpdateAsync(auctionItem);
        }
    }
}