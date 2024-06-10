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
        private readonly GenericDao<Wallet> _walletDao;

        public WalletRepository()
        {
            _walletDao = new GenericDao<Wallet>();
        }

        public async Task<Wallet> CreateWallet(Wallet wallet)
        {
            var result = await _walletDao.AddAsync(wallet);
            return result;
        }
    }
}
