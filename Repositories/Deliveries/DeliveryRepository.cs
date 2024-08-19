using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Deliveries
{
    public class DeliveryRepository : IDeliveryRepository
    {

        public async Task<Address> CreateDelivery(Address address)
        {
            return await GenericDao<Address>.Instance.AddAsync(address);
        }

        public async Task DeleteDelivery(Address address)
        {
            await GenericDao<Address>.Instance.DeleteAsync(address);
        }

        public Task UpdateRange(List<Address> list)
        {
            return GenericDao<Address>.Instance.UpdateRange(list);
        }

        public async Task<Address?> GetDeliveryById(Guid id)
        {
            return await GenericDao<Address>.Instance.GetQueryable().Include(c => c.Member).FirstOrDefaultAsync(c => c.AddressId.Equals(id));
        }

        public async Task<List<Address>> GetDeliveryByMemberId(Guid id)
        {
            var list = await GenericDao<Address>.Instance.GetQueryable().Include(c => c.Member).Where(c => c.MemberId.Equals(id)).ToListAsync();
            return list;
        }

        public async Task<Address> UpdateDelivery(Address address)
        {
            return await GenericDao<Address>.Instance.UpdateAsync(address);
        }
    }
}
