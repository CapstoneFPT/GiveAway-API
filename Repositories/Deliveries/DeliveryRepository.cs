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
        private readonly GenericDao<Address> _deliveryDao;

        public DeliveryRepository(GenericDao<Address> genericDao)
        {
            _deliveryDao = genericDao;
        }

        public async Task<Address> CreateDelivery(Address address)
        {
            return await _deliveryDao.AddAsync(address);
        }

        public async Task DeleteDelivery(Address address)
        {
            await _deliveryDao.DeleteAsync(address);
        }

        public async Task<Address> GetDeliveryById(Guid id)
        {
            return await _deliveryDao.GetQueryable().Include(c => c.Member).FirstOrDefaultAsync(c => c.AddressId.Equals(id));
        }

        public async Task<List<Address>> GetDeliveryByMemberId(Guid id)
        {
            var list = await _deliveryDao.GetQueryable().Include(c => c.Member).Where(c => c.MemberId.Equals(id)).ToListAsync();
            return list;
        }

        public async Task<Address> UpdateDelivery(Address address)
        {
            return await _deliveryDao.UpdateAsync(address);
        }
    }
}
