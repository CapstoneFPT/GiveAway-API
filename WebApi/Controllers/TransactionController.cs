using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
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

        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("export-excel")]
        [ProducesResponseType<FileContentResult>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportTransactionsToExcel([FromQuery] ExportTransactionsRequest request)
        {
            var result = await _transactionService.ExportTransactionsToExcel(request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("No transactions found in the specified date range",
                        ErrorType.ApiError, HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error exporting transactions", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return File(result.Value.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.Value.FileName);
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