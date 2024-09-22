using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Transactions;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PaginationResponse<TransactionResponse>>>> GetAllTransactions(
            [FromQuery] TransactionRequest transactionRequest)
        {
            var result = await _transactionService.GetAllTransaction(transactionRequest);
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
    }
}