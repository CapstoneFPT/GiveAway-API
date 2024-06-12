using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Wallet;
using BusinessObjects.Entities;

namespace Repositories.Wallets
{
    public interface IWalletRepository
    {
        Task<Wallet> CreateWallet(Wallet wallet);
        Task<Wallet> GetWalletByAccountId(Guid id);
        Task<Wallet> UpdateWallet(Wallet wallet);
    }
}
