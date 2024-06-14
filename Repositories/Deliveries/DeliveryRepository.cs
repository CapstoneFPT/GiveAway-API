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
        private readonly GenericDao<Delivery> _deliveryDao;

        public DeliveryRepository()
        {
            _deliveryDao = new GenericDao<Delivery>();
        }

        public async Task<Delivery> CreateDelivery(Delivery delivery)
        {
            return await _deliveryDao.AddAsync(delivery);
        }

        public async Task DeleteDelivery(Delivery delivery)
        {
            await _deliveryDao.DeleteAsync(delivery);
        }

        public async Task<Delivery> GetDeliveryById(Guid id)
        {
            return await _deliveryDao.GetQueryable().Include(c => c.Member).FirstOrDefaultAsync(c => c.DeliveryId.Equals(id));
        }

        public async Task<List<Delivery>> GetDeliveryByMemberId(Guid id)
        {
            var list = await _deliveryDao.GetQueryable().Include(c => c.Member).Where(c => c.MemberId.Equals(id)).ToListAsync();
            return list;
        }

        public async Task<Delivery> UpdateDelivery(Delivery delivery)
        {
            return await _deliveryDao.UpdateAsync(delivery);
        }
    }
}
