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
        Task<Result<List<DeliveryListResponse>>> GetAllDeliveriesByMemberId(Guid memberId);
        Task<Result<DeliveryListResponse>> CreateDelivery(Guid accountId, DeliveryRequest deliveryRequest);
        Task<Result<DeliveryListResponse>> UpdateDelivery(Guid deliveryId, UpdateDeliveryRequest deliveryRequest);
        Task<Result<string>> DeleteDelivery(Guid deliveryId);
        Task<DotNext.Result<DeliveryListResponse, ErrorCode>> SetAddressAsDefault(Guid deliveryId);
    }
}
