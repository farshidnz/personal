using Dapper.Contrib.Extensions;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Cashrewards3API.Features.Category
{
    [Table("Category")]
    public class CategoryModel
    {
        [Key]
        public int CategoryId { get; set; }

        public int? ClientId { get; set; }

        public string Name { get; set; }

        public int? Status { get; set; }

        public string HyphenatedString { get; set; }

        public int? RootCategoryId { get; set; }

        public string DisplayName { get; set; }

        public int Ranking { get; set; }

        public string MetaDescription { get; set; }

        public int MerchantCount { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HyphenatedString { get; set; }

        public string MetaDescription { get; set; }

        public int? Status { get; set; }

    }

    public class CategoryWithCountDTO : CategoryDto
    {
        public int MerchantCount { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status 
    {
        [Description("Deleted")]
        Deleted = 0,
        [Description("Active")]
        Active = 1,
        [Description("InActive")]
        InActive = 2,
        [Description("All")]
        All = 3
    }
}
