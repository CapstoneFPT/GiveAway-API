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
        Task<AuctionFashionItem> UpdateAuctionItemStatus(Guid auctionFashionItemId, FashionItemStatus fashionItemStatus);
    }
}
