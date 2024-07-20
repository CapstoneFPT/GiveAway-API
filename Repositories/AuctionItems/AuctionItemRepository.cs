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
        public async Task<AuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId,
            FashionItemStatus fashionItemStatus)
        {
            var auctionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == auctionFashionItemId);
            if (auctionItem is null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = fashionItemStatus;
            return await GenericDao<AuctionFashionItem>.Instance.UpdateAsync(auctionItem);
        }
    }
}