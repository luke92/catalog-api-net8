using CsvHelper.Configuration.Attributes;

namespace CatalogChallengeNet8.Application.Models
{
    public class ProductCategoryDto
    {
        [Index(0)]
        public required string ProductName { get; set; }
        [Index(1)]
        public required string ProductCode { get; set; }
        [Index(2)]
        public required string CategoryName { get; set; }
        [Index(3)]
        public required string CategoryCode { get; set; }
    }
}
