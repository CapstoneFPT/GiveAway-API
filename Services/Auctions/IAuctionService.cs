using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;

namespace Services.Auctions
{
    public interface IAuctionService
    {
        Task CreateAuction(CreateAuctionRequest request);
    }
}
