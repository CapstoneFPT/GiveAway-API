using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using Repositories.AuctionDeposits;

namespace Services.AuctionDeposits
{
    public class AuctionDepositService : IAuctionDepositService
    {
        private readonly IAuctionDepositRepository _auctionDepositRepository;

        public AuctionDepositService(IAuctionDepositRepository auctionDepositRepository)
        {
            _auctionDepositRepository = auctionDepositRepository;
        }

        public async Task<bool> CheckDepositAvailable(Guid auctionId, Guid requestMemberId)
        {
            var result = await _auctionDepositRepository.GetSingleDeposit<AuctionDeposit>(predicate: deposit => deposit.AuctionId == auctionId && deposit.MemberId == requestMemberId, selector:null);
            
            return result != null;
        }
    }
}
