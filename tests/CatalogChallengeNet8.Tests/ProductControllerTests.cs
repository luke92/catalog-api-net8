using CatalogChallengeNet8.API.Controllers;
using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Models;
using Moq;
using Microsoft.AspNetCore.Mvc;

public class ProductControllerTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _controller = new ProductController(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProducts_ReturnsOkResult_WhenValidParametersProvided()
    {
        // Arrange
        var sampleProducts = new List<ProductModel>
    {
        new ProductModel { Id = Guid.NewGuid(), Name = "Product A", Code = "A123", CategoryCode = "Cat1" },
        new ProductModel { Id = Guid.NewGuid(), Name = "Product B", Code = "B456", CategoryCode = "Cat2" }
    };
        _mockRepo.Setup(repo => repo.GetProductsAsync(1, 10, null, null, null, SortOrder.Asc))
            .ReturnsAsync((sampleProducts, sampleProducts.Count));

        // Act
        var result = await _controller.GetProducts(1, 10, null, null, null, SortOrder.Asc);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PagedResponse<ProductModel>>(okResult.Value);
        Assert.Equal(2, response.TotalCount);
    }

    [Fact]
    public async Task GetProducts_ReturnsBadRequest_WhenPageIsZero()
    {
        // Act
        var result = await _controller.GetProducts(0, 10, null, null, null, SortOrder.Asc);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page and pageSize must be greater than zero.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetProducts_ReturnsBadRequest_WhenPageSizeIsZero()
    {
        // Act
        var result = await _controller.GetProducts(1, 0, null, null, null, SortOrder.Asc);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page and pageSize must be greater than zero.", badRequestResult.Value);
    }
}
