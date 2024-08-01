using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.Accounts;
using Repositories.ConsignSaleDetails;
using Repositories.ConsignSales;
using Services.Emails;

namespace Services.ConsignSales
{
    public class ConsignSaleService : IConsignSaleService
    {
        private readonly IConsignSaleRepository _consignSaleRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IConsignSaleDetailRepository _consignSaleDetailRepository;
        private readonly IEmailService _emailService;

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository, IAccountRepository accountRepository,
            IConsignSaleDetailRepository consignSaleDetailRepository, IEmailService emailService)
        {
            _consignSaleRepository = consignSaleRepository;
            _accountRepository = accountRepository;
            _consignSaleDetailRepository = consignSaleDetailRepository;
            _emailService = emailService;
        }

        public async Task<Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId,
            ApproveConsignSaleRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
            var consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (consign == null)
            {
                response.Messages = ["Can not find the consign"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            if (!consign.Status.Equals(ConsignSaleStatus.Pending))
            {
                response.Messages = ["This consign is not allowed to approval"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            if (!request.Status.Equals(ConsignSaleStatus.AwaitDelivery) &&
                !request.Status.Equals(ConsignSaleStatus.Rejected))
            {
                response.Messages = ["Status not available"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = await _consignSaleRepository.ApprovalConsignSale(consignId, request.Status);
            response.Messages = ["Approval successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<ConsignSaleResponse>> ConfirmReceivedFromShop(Guid consignId,
            ConfirmReceivedConsignRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
            var consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (consign == null)
            {
                response.Messages = ["Consign is not found"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            if (!consign.Status.Equals(ConsignSaleStatus.AwaitDelivery))
            {
                response.Messages = ["This consign is not allowed to confirm"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            var result = await _consignSaleRepository.ConfirmReceivedFromShop(consignId,request);
            await _emailService.SendEmailConsignSale(consignId);
            response.Data = result;
            response.Messages = ["Confirm received successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId,
            CreateConsignSaleRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
            //check account co' active hay ko
            var account = await _accountRepository.GetAccountById(accountId);
            if (account == null || account.Status.Equals(AccountStatus.Inactive) ||
                account.Status.Equals(AccountStatus.NotVerified))
            {
                response.Messages = ["This account is not available to consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            //check list consign status pending co' dat gioi han. = 5 chua
            var listconsignpending = await _consignSaleRepository.GetAllConsignPendingByAccountId(accountId);
            if (listconsignpending.Count >= 5)
            {
                response.Messages = ["You have reached the consignment limit"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            //tao moi' 1 consign form
            var consign = await _consignSaleRepository.CreateConsignSale(accountId, request);
            if (consign == null)
            {
                response.Messages = ["There is an error. Can not find consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = consign;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId,
            CreateConsignSaleByShopRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
            var isMemberExisted = await _accountRepository.FindUserByPhone(request.Phone);
            if (isMemberExisted != null)
            {
                var account = await _accountRepository.GetAccountById(isMemberExisted.AccountId);
                if (account == null || account.Status.Equals(AccountStatus.Inactive) ||
                    account.Status.Equals(AccountStatus.NotVerified))
                {
                    response.Messages = ["This account is not available to consign"];
                    response.ResultStatus = ResultStatus.Error;
                    return response;
                }
            }

            //tao moi' 1 consign form
            var consign = await _consignSaleRepository.CreateConsignSaleByShop(shopId, request);
            if (consign == null)
            {
                response.Messages = ["There is an error. Can not find consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            
            
            response.Data = consign;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId,
            ConsignSaleRequest request)
        {
            var response = new Result<PaginationResponse<ConsignSaleResponse>>();
            var listConsign = await _consignSaleRepository.GetAllConsignSale(accountId, request);
            if (listConsign == null)
            {
                response.Messages = ["You don't have any consignment"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Data = listConsign;
            response.Messages = ["There are " + listConsign.TotalCount + " consignment"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSalesByShopId(Guid shopId,
            ConsignSaleRequestForShop request)
        {
            var response = new Result<PaginationResponse<ConsignSaleResponse>>();
            var listConsign = await _consignSaleRepository.GetAllConsignSaleByShopId(shopId, request);
            if (listConsign.TotalCount == 0)
            {
                response.Messages = ["You don't have any consignment"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Data = listConsign;
            response.Messages = ["There are " + listConsign.TotalCount + " consignment"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId)
        {
            var response = new Result<ConsignSaleResponse>();
            var Consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (Consign == null)
            {
                response.Messages = ["Consignment is not found"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = Consign;
            response.Messages = ["Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<List<ConsignSaleDetailResponse>>> GetConsignSaleDetailsByConsignSaleId(
            Guid consignsaleId)
        {
            var result = await _consignSaleDetailRepository.GetConsignSaleDetailsByConsignSaleId(consignsaleId);
            var response = new Result<List<ConsignSaleDetailResponse>>()
            {
                Data = result,
                Messages = new[] { "There are " + result.Count + " items in this consign" },
                ResultStatus = ResultStatus.Success,
            };
            return response;
        }

        
    }
}