using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CatalogChallengeNet8.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Retrieves a paginated list of products.
        /// </summary>
        /// <param name="page">Page number (must be greater than 0).</param>
        /// <param name="pageSize">Number of products per page (must be greater than 0).</param>
        /// <param name="categoryCode">Optional filter by category code.</param>
        /// <param name="productCode">Optional filter by product code.</param>
        /// <param name="sortBy">Field to sort by (e.g., "name", "code").</param>
        /// <param name="sortOrder">Sort order: "asc" or "desc".</param>
        /// <returns>A paginated list of products.</returns>
        /// <response code="200">Returns the paginated list of products.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<ProductModel>), 200)]
        [ProducesResponseType(400)]
        [SwaggerOperation(Summary = "Get paginated list of products", Description = "Retrieves a paginated list of products with optional filters and sorting.")]
        public async Task<ActionResult<PagedResponse<ProductModel>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categoryCode = null,
            [FromQuery] string? productCode = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] SortOrder? sortOrder = SortOrder.Asc
        )
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than zero.");
            }

            var (products, totalCount) = await _repository.GetProductsAsync(page, pageSize, productCode, categoryCode, sortBy, sortOrder);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var response = new PagedResponse<ProductModel>
            {
                Items = products,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(response);
        }
    }
}
