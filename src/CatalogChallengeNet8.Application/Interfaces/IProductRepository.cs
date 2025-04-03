using CatalogChallengeNet8.Application.Models;

namespace CatalogChallengeNet8.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<(IEnumerable<ProductModel>, int)> GetProductsAsync(
            int page, int pageSize, string? productCode, string? categoryCode, 
            string? sortBy, SortOrder? sortOrder
        );
    }
}
