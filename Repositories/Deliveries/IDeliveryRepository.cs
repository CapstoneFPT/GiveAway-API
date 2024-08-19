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
        Task<List<Address>> GetDeliveryByMemberId(Guid id);
        Task<Address> CreateDelivery(Address address);
        Task<Address> UpdateDelivery(Address address);
        Task<Address?> GetDeliveryById(Guid id);
        Task DeleteDelivery(Address address);
        Task UpdateRange(List<Address> list);
    }
}
