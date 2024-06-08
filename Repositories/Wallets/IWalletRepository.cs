using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;

namespace Repositories.Wallets
{
    public interface IWalletRepository
    {
        Task CreateWallet(Wallet wallet);
    }
}
