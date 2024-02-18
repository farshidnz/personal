using Cashrewards3API.Features.Transaction.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Transaction
{
    [Authorize("InternalPolicy")]
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class TransactionInternalController : ControllerBase
    {
        private readonly ISaleAdjustmentTransactionService _svc;

        public TransactionInternalController(ISaleAdjustmentTransactionService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// get sale transactions 
        /// </summary>
        /// <param name="clickId"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transaction/sale-adjustment")]
        [ProducesResponseType(typeof(List<SaleAdjustmentTransactionResultModel>), 200)]
        public async Task<ActionResult<List<SaleAdjustmentTransactionResultModel>>> GetTransactionDetailForSaleAdjustment(int? clickId, int transactionId)
        {
            var response = await _svc.GetTransactionDetailAsync(transactionId, clickId);
            return Ok(response.ToList());
        }
    }
}
