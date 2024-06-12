using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Wallet;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Wallet> GetWalletByAccountId(Guid id)
        {
            var wallet = await _walletDao.GetQueryable().Include(c => c.Member).Where(c => c.MemberId.Equals(id))
                .AsNoTracking().FirstOrDefaultAsync();
            return wallet;
        }

        public async Task<Wallet> UpdateWallet(Wallet wallet)
        {
            await _walletDao.UpdateAsync(wallet);
            return wallet;
        }
    }
}
