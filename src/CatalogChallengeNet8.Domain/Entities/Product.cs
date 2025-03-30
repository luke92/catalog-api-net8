namespace CatalogChallengeNet8.Domain.Entities
{
    public class Product: BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;

        public Category? Category { get; set; }
    }
}
