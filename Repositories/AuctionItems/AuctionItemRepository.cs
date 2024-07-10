using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;

namespace Repositories.AuctionItems
{
    public class AuctionItemRepository : IAuctionItemRepository
    {
        private readonly GenericDao<AuctionFashionItem> _auctionFashionItemDao;

        public AuctionItemRepository(GenericDao<AuctionFashionItem> auctionFashionItemDao)
        {
            _auctionFashionItemDao = auctionFashionItemDao;
        }

        public Task<AuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId,
            FashionItemStatus fashionItemStatus)
        {
            var auctionItem = _auctionFashionItemDao.GetQueryable()
                .FirstOrDefault(x => x.ItemId == auctionFashionItemId);
            if (auctionItem is null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = fashionItemStatus;
            return _auctionFashionItemDao.UpdateAsync(auctionItem);
        }
    }
}