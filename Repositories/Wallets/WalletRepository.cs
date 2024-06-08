using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using Dao;

namespace Repositories.Wallets
{
    public class WalletRepository : IWalletRepository
    {
        private readonly GenericDao<Wallet> _wallet;

        public WalletRepository()
        {
            _wallet = new GenericDao<Wallet>();
        }

        public Task CreateWallet(Wallet wallet)
        {
            _wallet.AddAsync(wallet);
            _wallet.SaveChangesAsync();
            return Task.FromResult(wallet);
        }
    }
}
