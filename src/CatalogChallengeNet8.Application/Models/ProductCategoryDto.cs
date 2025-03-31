using CsvHelper.Configuration.Attributes;

namespace CatalogChallengeNet8.Application.Models
{
    public class ProductCategoryDto
    {
        [Index(0)]
        public string ProductName { get; set; }
        [Index(1)]
        public string ProductCode { get; set; }
        [Index(2)]
        public string CategoryName { get; set; }
        [Index(3)]
        public string CategoryCode { get; set; }
    }
}
