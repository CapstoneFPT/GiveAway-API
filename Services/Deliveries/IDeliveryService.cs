using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Deliveries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Deliveries
{
    public interface IDeliveryService
    {
        Task<Result<List<DeliveryResponse>>> GetAllDeliveriesByMemberId(Guid memberId);
        Task<Result<DeliveryResponse>> CreateDelivery(Guid accountId, DeliveryRequest deliveryRequest);
        Task<Result<DeliveryResponse>> UpdateDelivery(Guid deliveryId, DeliveryRequest deliveryRequest);
        Task<Result<string>> DeleteDelivery(Guid deliveryId);
    }
}
