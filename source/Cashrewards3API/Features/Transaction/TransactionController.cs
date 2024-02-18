using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Transaction.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashrewards3API.Features.Transaction
{
    [Authorize]
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
       
        #region Constructor(s)

        private readonly ITransactionService _svc;
        private readonly IRequestContext reqestContext;
        private readonly ILogger<TransactionController> _logger;
        private readonly IMemberTransactionService memberTransactionServic;
        const int DefaultLimit = 20;
        const int DefaultOffset = 0;


        public TransactionController(ITransactionService svc,
                            IRequestContext reqestContext,
                            ILogger<TransactionController> logger,
                            IMemberTransactionService memberTransactionServic)
        {
            _svc = svc;
            this.reqestContext = reqestContext;
            _logger = logger;
            this.memberTransactionServic = memberTransactionServic;
        }

        #endregion

        /// <summary>
        /// Post card transaction
        /// </summary>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Route("transaction")]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        public async Task<ActionResult> CreateTransaction([FromBody] TransactionDto transaction)
        {
            if (transaction == null)
            {
                return BadRequest();
            }
            var message = await _svc.CreateTransactionEventAsync(transaction);
            return Ok(message);
        }

        /// <summary>
        /// get member transactions 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transactions")]
        [ProducesResponseType(typeof(PagedList<MemberTransactionMerchantModel>), 200)]
        public async Task<ActionResult> GetMemberTransactions(int limit = DefaultLimit, int offset = DefaultOffset)
        {
            int memberId = await reqestContext.GetMemberidFromDynamodbasync();
            if (memberId <= 0)
                return BadRequest();

            var model = new MemberTransactionRequestInfoModel()
            {
                MemberId = memberId,
                Offset = offset,
                Limit = limit
            };
            var response = await memberTransactionServic.GetTransactionsForMemberAsync(model);
            return Ok(response);
        }
    }
}
