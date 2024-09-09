using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Repositories.AuctionItems
{
    public interface IAuctionItemRepository
    {
        Task<IndividualAuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId, FashionItemStatus fashionItemStatus);
        IQueryable<IndividualAuctionFashionItem> GetQueryable();
    }
}
