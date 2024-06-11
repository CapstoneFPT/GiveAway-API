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

        public async Task<Wallet> GetWalletByAccountId(Guid id)
        {
            var wallet = await _wallet.GetQueryable().Include(c => c.Member).Where(c => c.MemberId.Equals(id))
                .AsNoTracking().FirstOrDefaultAsync();
            return wallet;
        }

        public async Task<Wallet> UpdateWallet(Wallet wallet)
        {
            _wallet.UpdateAsync(wallet);
            await _wallet.SaveChangesAsync();
            return wallet;
        }
    }
}
