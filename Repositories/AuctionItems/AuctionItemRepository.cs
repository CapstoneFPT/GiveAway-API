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
        
        public async Task<IndividualAuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId,
            FashionItemStatus fashionItemStatus)
        {
            var auctionItem = await GenericDao<IndividualAuctionFashionItem>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == auctionFashionItemId);
            if (auctionItem is null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = fashionItemStatus;
            return await GenericDao<IndividualAuctionFashionItem>.Instance.UpdateAsync(auctionItem);
        }

        public IQueryable<IndividualAuctionFashionItem> GetQueryable()
        {
            return GenericDao<IndividualAuctionFashionItem>.Instance.GetQueryable(); 
        }
    }
}