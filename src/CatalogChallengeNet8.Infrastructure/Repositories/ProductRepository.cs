using System.Linq.Expressions;
using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Models;
using CatalogChallengeNet8.Domain.Entities;
using CatalogChallengeNet8.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogChallengeNet8.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        //private readonly IMapper _mapper;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<ProductModel>, int)> GetProductsAsync(
            int page, int pageSize, string? productCode, string? categoryCode, string? sortBy, SortOrder? sortOrder)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(categoryCode))
            {
                query = query.Where(p => p.CategoryCode.Contains(categoryCode));
            }

            if (!string.IsNullOrEmpty(productCode))
            {
                query = query.Where(p => p.Code.Contains(productCode));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortOrder == SortOrder.Desc
                    ? query.OrderByDescending(GetSortingExpression(sortBy))
                    : query.OrderBy(GetSortingExpression(sortBy));
            }

            var totalProducts = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //We can use AutoMapper for do this instead
            //var productModels = _mapper.Map<IEnumerable<ProductModel>>(products);
            var productModels = products.Select(p => new ProductModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                CategoryCode = p.CategoryCode
            });

            return (productModels, totalProducts);
        }

        private static Expression<Func<Product, object>> GetSortingExpression(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => p => p.Name,
                "code" => p => p.Code,
                "categorycode" => p => p.CategoryCode,
                _ => p => p.Id // Default sorting
            };
        }
    }
}
