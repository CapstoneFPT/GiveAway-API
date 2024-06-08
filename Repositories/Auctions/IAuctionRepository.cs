using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;

namespace Repositories.Auctions
{
    public interface IAuctionRepository
    {
        Task CreateAuction(CreateAuctionRequest request);
    }
}
