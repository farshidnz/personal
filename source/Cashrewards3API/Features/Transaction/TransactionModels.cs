using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Cashrewards3API.Features.Transaction
{
  public class TransactionDto
  {
    public string Type { get; set; }

    public string RefNum { get; set; }

    public string AuthMerchantId { get; set; }

    public string AcquirerICA { get; set; }

    public int? LocationId { get; set; }
    public string Timestamp { get; set; }
    public decimal Amount { get; set; }

    public string MemberId { get; set; }
  }

  public class TransactionModel : TransactionDto
  {
    public string ClientId { get; set; }
  }
}
