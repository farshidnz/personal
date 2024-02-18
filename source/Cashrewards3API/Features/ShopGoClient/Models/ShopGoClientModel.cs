
using Dapper.Contrib.Extensions;

namespace Cashrewards3API.Features.ShopGoClient.Models
{
    [Table("Client")]
    public class ShopGoClientModel
    {
        [Key]
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientKey { get; set; }
        public int Status { get; set; }
    }
}