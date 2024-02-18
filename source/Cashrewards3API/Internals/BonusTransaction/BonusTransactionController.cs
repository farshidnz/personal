using System.Threading.Tasks;
using Cashrewards3API.Internals.BonusTransaction.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashrewards3API.Internals.BonusTransaction
{
    /// <summary>
    /// An internal route for promotion microservice to use inorder
    /// to interactive with MSSQL.
    /// </summary>
    [Authorize(Policy = "InternalPolicy")]
    [ApiController]
    [Route("api/v1/internal")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly IBonusTransactionService _bonusTransactionService;

        public TransactionController(IBonusTransactionService bonusTransactionService)
        {
            _bonusTransactionService = bonusTransactionService;
        }


        /// <summary>
        /// Gets bonus transaction by id.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("bonus-transaction/{transactionId:int}")]
        [ProducesResponseType(typeof(BonusTransactionResultModel), 200)]
        public async Task<ActionResult> GetBonusTransactionById(int transactionId)
        {
            var bonusTransaction = await _bonusTransactionService.GetById(transactionId);

            if (bonusTransaction != null)
            {
                return Ok(bonusTransaction);
            }

            return NotFound($"Transaction {transactionId} was not found");
        }


        /// <summary>
        /// Creates a new bonus transaction and dependant models which includes a
        /// transaction tier, merchant tier (if not existing) and merchant tier client.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns></returns>
        [HttpPost]
        [Route("bonus-transaction")]
        [ProducesResponseType(typeof(BonusTransactionResultModel), 200)]
        public async Task<ActionResult> CreateBonusTransaction(
            [FromBody] CreateBonusTransactionRequestModel requestModel)
            => Ok(await _bonusTransactionService.CreateBonusTransaction(requestModel));


        /// <summary>
        /// Approves an existing bonus transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns></returns>
        [HttpPut]
        [Route("approve-bonus-transaction/{transactionId:int}")]
        public async Task<ActionResult> ApproveBonusTransaction(int transactionId)
        {
            var response = await _bonusTransactionService.ApproveBonusTransaction(transactionId);
            if (response != null)
            {
                return Ok(response);
            }

            return NotFound("Transaction not found to Approve");
        }

        /// <summary>
        /// Declines an existing bonus transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns></returns>
        [HttpPut]
        [Route("decline-bonus-transaction/{transactionId:int}")]
        public async Task<ActionResult> DeclineBonusTransaction(int transactionId)
        {
            var response = await _bonusTransactionService.DeclineBonusTransaction(transactionId);
            if (response != null)
            {
                return Ok(response);
            }

            return NotFound("Transaction not found to Decline");
        }


        [HttpGet]
        [Route("qualifying-transactions")]
        public async Task<ActionResult> GetQualifyingTransactions([FromBody]QualifyingTransactionsRequestModel request)
        {
            var response = await _bonusTransactionService.GetQualifyingTransactions(request);
            return Ok(response);
        }

    }
}