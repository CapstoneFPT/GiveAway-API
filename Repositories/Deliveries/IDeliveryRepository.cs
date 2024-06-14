using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Deliveries
{
    public interface IDeliveryRepository
    {
        Task<List<Delivery>> GetDeliveryByMemberId(Guid id);
        Task<Delivery> CreateDelivery(Delivery delivery);
        Task<Delivery> UpdateDelivery(Delivery delivery);
        Task<Delivery> GetDeliveryById(Guid id);
        Task DeleteDelivery(Delivery delivery);
    }
}
