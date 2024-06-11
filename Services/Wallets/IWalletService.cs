using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Wallets
{
    public interface IWalletService
    {
        Task<Result<WalletResponse>> GetWalletByAccountId(Guid accountId);
        Task<Result<WalletResponse>> UpdateWallet(Guid id, UpdateWalletRequest request);
    }
}
