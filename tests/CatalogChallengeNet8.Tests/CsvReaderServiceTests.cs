using CatalogChallengeNet8.Application.Models;
using CatalogChallengeNet8.Application.Services;

public class CsvReaderServiceTests
{
    [Fact]
    public async Task ReadCsvAsync_ValidFile_ReturnsExpectedRecords()
    {
        // Arrange
        var csvContent = "Product1, P001, Category1, C001\nProduct2, P002, Category2, C002";
        var filePath = "test.csv";

        await File.WriteAllTextAsync(filePath, csvContent);

        var csvReaderService = new CsvReaderService();

        // Act
        var records = csvReaderService.ReadCsv<ProductCategoryDto>(filePath);

        // Assert
        Assert.Equal(2, records.Count);
        Assert.Equal("Product1", records[0].ProductName);
        Assert.Equal("P001", records[0].ProductCode);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task ReadCsvAsync_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var filePath = "empty.csv";
        await File.WriteAllTextAsync(filePath, "");

        var csvReaderService = new CsvReaderService();

        // Act
        var records = csvReaderService.ReadCsv<string>(filePath);

        // Assert
        Assert.Empty(records);

        // Cleanup
        File.Delete(filePath);
    }
}
