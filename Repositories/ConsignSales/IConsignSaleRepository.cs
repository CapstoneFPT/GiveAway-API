﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.ConsignSales
{
    public interface IConsignSaleRepository
    {
        Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSale(Guid accountId, ConsignSaleRequest request);
        Task<ConsignSaleResponse> GetConsignSaleById(Guid consignId);
    }
}
