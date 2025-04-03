using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Models;
using CatalogChallengeNet8.Domain.Entities;
using CatalogChallengeNet8.DataImporter;

public class DataImportOrchestratorTests
{
    private readonly Mock<ICsvReaderService> _csvReaderMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<Category>> _categoryRepoMock;
    private readonly Mock<ILogger<DataImportOrchestrator>> _loggerMock;
    private readonly Mock<IOptions<ImportSettings>> _settingsMock;
    private readonly DataImportOrchestrator _orchestrator;

    public DataImportOrchestratorTests()
    {
        _csvReaderMock = new Mock<ICsvReaderService>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _loggerMock = new Mock<ILogger<DataImportOrchestrator>>();
        _settingsMock = new Mock<IOptions<ImportSettings>>();

        _settingsMock.Setup(s => s.Value).Returns(new ImportSettings { StopOnError = false });

        _orchestrator = new DataImportOrchestrator(
            _csvReaderMock.Object,
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _loggerMock.Object,
            _settingsMock.Object
        );
    }

    [Fact]
    public async Task ImportDataAsync_ShouldLogWarning_WhenCsvIsEmpty()
    {
        // Arrange
        _csvReaderMock.Setup(r => r.ReadCsv<ProductCategoryDto>(It.IsAny<string>(),',',false)).Returns(new List<ProductCategoryDto>());

        // Act
        await _orchestrator.ImportDataAsync("test.csv");

        // Assert
        _loggerMock.Verify(log => log.Log(
            LogLevel.Warning, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ImportDataAsync_ShouldCallRepositories_WhenDataIsValid()
    {
        // Arrange
        var sampleData = new List<ProductCategoryDto>
        {
            new() { ProductName = "Product1", ProductCode = "P1", CategoryName = "Category1", CategoryCode = "C1" }
        };

        _csvReaderMock.Setup(r => r.ReadCsv<ProductCategoryDto>(It.IsAny<string>(), ',', false)).Returns(sampleData);
        _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        // Act
        await _orchestrator.ImportDataAsync("test.csv");

        // Assert
        _categoryRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Once);
        _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>()), Times.Once);
        
    }

    [Fact]
    public async Task ImportDataAsync_ShouldNotProceed_WhenFetchingExistingCodesFails()
    {
        // Arrange
        _csvReaderMock.Setup(r => r.ReadCsv<ProductCategoryDto>(It.IsAny<string>(), ',', false))
            .Returns(new List<ProductCategoryDto>
            {
                new() { ProductName = "Product1", ProductCode = "P1", CategoryName = "Category1", CategoryCode = "C1" }
            });

        _productRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        await _orchestrator.ImportDataAsync("test.csv");

        // Assert
        _loggerMock.Verify(log => log.Log(
            LogLevel.Error, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
