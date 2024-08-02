using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Entities;
using Repositories.Accounts;
using Repositories.Deliveries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Utils;

namespace Services.Deliveries
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public DeliveryService(IDeliveryRepository delivery, IMapper mapper, IAccountRepository accountRepository)
        {
            _deliveryRepository = delivery;
            _mapper = mapper;
            _accountRepository = accountRepository;
        }

        public async Task<Result<DeliveryResponse>> CreateDelivery(Guid accountId, DeliveryRequest deliveryRequest)
        {
            var response = new Result<DeliveryResponse>();
            var list = await _deliveryRepository.GetDeliveryByMemberId(accountId);
            if(list.Count >= 5)
            {
                response.Messages = ["Maxium deliveries! Please delete or update"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            var delivery = new Address();
            delivery.MemberId = accountId;
            delivery.CreatedDate = DateTime.UtcNow;
            if (list.Count == 0)
            {
                delivery.IsDefault = true;
            }
            else
            {
                delivery.IsDefault = false;
            }
            var request = _mapper.Map(deliveryRequest, delivery);
            response.Data = _mapper.Map<DeliveryResponse>(await _deliveryRepository.CreateDelivery(delivery));
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<Result<string>> DeleteDelivery(Guid deliveryId)
        {
            var response = new Result<string>();
            var delivery = await _deliveryRepository.GetDeliveryById(deliveryId);
            if (delivery == null)
            {
                throw new AddressNotFoundException();
            }
            await _deliveryRepository.DeleteDelivery(delivery);
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Delete successfully"];
            return response;
        }

        public async Task<Result<List<DeliveryResponse>>> GetAllDeliveriesByMemberId(Guid memberId)
        {
            var response = new Result<List<DeliveryResponse>>();
            if(await _accountRepository.GetAccountById(memberId) is null)
            {
                throw new AccountNotFoundException();
            }
            var list = await _deliveryRepository.GetDeliveryByMemberId(memberId);
            if(list == null)
            {
                response.Data = new List<DeliveryResponse>();
                response.Messages = ["Empty! Please create one"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            response.Data = _mapper.Map<List<DeliveryResponse>>(list);
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Choose one to receive delivery"];
            return response;
        }

        public async Task<Result<DeliveryResponse>> UpdateDelivery(Guid deliveryId, UpdateDeliveryRequest deliveryRequest)
        {
            var response = new Result<DeliveryResponse>();
            var delivery = await _deliveryRepository.GetDeliveryById(deliveryId);
            if(delivery == null)
            {
                response.Messages = ["Delivery is not existed"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            var newdata = _mapper.Map(deliveryRequest, delivery);
            response.Data = _mapper.Map<DeliveryResponse>(await _deliveryRepository.UpdateDelivery(newdata));
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Update successfully"];
            return response;
        }
    }
}
