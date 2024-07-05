using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.ConsignSales;

namespace Services.ConsignSales
{
    public class ConsignSaleService : IConsignSaleService
    {
        private readonly IConsignSaleRepository _consignSaleRepository;

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository)
        {
            _consignSaleRepository = consignSaleRepository;
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
