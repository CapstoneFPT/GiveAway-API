﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.Accounts;
using Repositories.ConsignSales;

namespace Services.ConsignSales
{
    public class ConsignSaleService : IConsignSaleService
    {
        private readonly IConsignSaleRepository _consignSaleRepository;
        private readonly IAccountRepository _accountRepository;

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository, IAccountRepository accountRepository)
        {
            _consignSaleRepository = consignSaleRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId, ConsignSaleStatus status)
        {
            var response = new Result<ConsignSaleResponse>();
            var consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (consign == null)
            {
                response.Messages = ["Can not find the consign"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }else if(!consign.Status.Equals(ConsignSaleStatus.Pending))
            {
                response.Messages = ["This consign is not allowed to aprroval"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            else if(!status.Equals(ConsignSaleStatus.AwaitDelivery) && !status.Equals(ConsignSaleStatus.Rejected))
            {
                response.Messages = ["Status not available"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            response.Data = await _consignSaleRepository.ApprovalConsignSale(consignId,status);
            response.Messages = ["Approval successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request)
        {
            try
            {
                var response = new Result<ConsignSaleResponse>();
                //check account co' active hay ko
                var account = await _accountRepository.GetAccountById(accountId);
                if (account == null || account.Status.Equals(AccountStatus.Inactive) || account.Status.Equals(AccountStatus.NotVerified))
                {
                    response.Messages = ["This account is not available to consign"];
                    response.ResultStatus = ResultStatus.Error;
                    return response;
                }
                //check list consign status pending co' dat gioi han. = 5 chua
                var listconsignpending = await _consignSaleRepository.GetAllConsignPendingByAccountId(accountId);
                if(listconsignpending.Count() >= 5)
                {
                    response.Messages = ["You have reached the consignment limit"];
                    response.ResultStatus = ResultStatus.Error;
                    return response;
                }
                //tao moi' 1 consign form
                var consign = await _consignSaleRepository.CreateConsignSale(accountId, request);
                if(consign == null)
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<ConsignSaleResponse>>();
                var listConsign = await _consignSaleRepository.GetAllConsignSale(accountId, request);
                if(listConsign == null)
                {
                    response.Messages = ["You don't have any consignment"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }
                response.Data = listConsign;
                response.Messages = ["There are " + listConsign.TotalCount + " consignment"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
