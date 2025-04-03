// En CatalogChallengeNet8.DataImporter/DataImportOrchestrator.cs
using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Models; // Using ProductCategoryDto
using CatalogChallengeNet8.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CatalogChallengeNet8.DataImporter;
using Microsoft.Extensions.Options; // For DbUpdateException

public class DataImportOrchestrator
{
    private readonly ICsvReaderService _csvReaderService;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ILogger<DataImportOrchestrator> _logger;
    private readonly ImportSettings _settings;

    public DataImportOrchestrator(
        ICsvReaderService csvReaderService,
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository,
        ILogger<DataImportOrchestrator> logger,
        IOptions<ImportSettings> settings)
    {
        _csvReaderService = csvReaderService ?? throw new ArgumentNullException(nameof(csvReaderService));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task ImportDataAsync(string filePath)
    {
        _logger.LogInformation("Starting data import process from file: {FilePath}", filePath);

        var dtos = _csvReaderService.ReadCsv<ProductCategoryDto>(filePath);

        if (!ValidateCsvContent(dtos)) return;

        var (existingProductCodes, existingCategoryCodes) = await FetchExistingCodesAsync();
        if (existingProductCodes == null || existingCategoryCodes == null) return;

        var validDtos = ValidateRecords(dtos, existingProductCodes, existingCategoryCodes);
        if (!validDtos.Any()) return;

        await SaveToDatabaseAsync(validDtos);
    }

    private bool ValidateCsvContent(List<ProductCategoryDto> dtos)
    {
        if (dtos == null || !dtos.Any())
        {
            _logger.LogWarning("No data found in the CSV file or the file could not be read.");
            return false;
        }
        _logger.LogInformation("Read {DtoCount} records from CSV file.", dtos.Count);
        return true;
    }

    private async Task<(HashSet<string>, HashSet<string>)> FetchExistingCodesAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            return (
                products.Select(p => p.Code.ToLowerInvariant()).ToHashSet(),
                categories.Select(c => c.Code.ToLowerInvariant()).ToHashSet()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching existing codes from the database. Aborting import.");
            return (null, null);
        }
    }

    private List<ProductCategoryDto> ValidateRecords(List<ProductCategoryDto> dtos, HashSet<string> existingProductCodes, HashSet<string> existingCategoryCodes)
    {
        var batchProductCodes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        var batchCategoryCodes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        var validDtos = new List<ProductCategoryDto>();
        var invalidDtos = new List<ProductCategoryDto>();
        var validationErrors = new List<string>();
        var stopOnError = _settings.StopOnError;

        _logger.LogInformation("Starting validation of {DtoCount} records...", dtos.Count);
        foreach (var (dto, index) in dtos.Select((item, idx) => (item, idx)))
        {
            var errors = ValidateDto(dto, index + 1, existingProductCodes, existingCategoryCodes, batchProductCodes, batchCategoryCodes);
            if (errors.Any())
            {
                invalidDtos.Add(dto);
                validationErrors.AddRange(errors);
                if (stopOnError) return new List<ProductCategoryDto>();
            }
            else
            {
                validDtos.Add(dto);
            }
        }
        LogValidationResults(validDtos.Count, invalidDtos.Count, validationErrors);
        return validDtos;
    }

    private List<string> ValidateDto(ProductCategoryDto dto, int lineNumber, HashSet<string> existingProductCodes, HashSet<string> existingCategoryCodes, HashSet<string> batchProductCodes, HashSet<string> batchCategoryCodes)
    {
        var errors = new List<string>();
        var currentProductCode = dto.ProductCode?.Trim().ToLowerInvariant() ?? string.Empty;
        var currentCategoryCode = dto.CategoryCode?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dto.ProductName)) errors.Add($"L{lineNumber}: Product Name is missing.");
        if (string.IsNullOrWhiteSpace(dto.ProductCode)) errors.Add($"L{lineNumber}: Product Code is missing.");
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) errors.Add($"L{lineNumber}: Category Name is missing.");
        if (string.IsNullOrWhiteSpace(dto.CategoryCode)) errors.Add($"L{lineNumber}: Category Code is missing.");

        if (!string.IsNullOrWhiteSpace(dto.ProductCode) && (existingProductCodes.Contains(currentProductCode) || !batchProductCodes.Add(currentProductCode)))
            errors.Add($"L{lineNumber}: Product Code '{dto.ProductCode}' already exists.");

        if (!string.IsNullOrWhiteSpace(dto.CategoryCode) && (existingCategoryCodes.Contains(currentCategoryCode) || !batchCategoryCodes.Add(currentCategoryCode)))
            errors.Add($"L{lineNumber}: Category Code '{dto.CategoryCode}' already exists.");

        return errors;
    }

    private void LogValidationResults(int validCount, int invalidCount, List<string> validationErrors)
    {
        _logger.LogInformation("Validation completed. Valid records: {ValidCount}, Invalid records: {InvalidCount}", validCount, invalidCount);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation errors:");
            validationErrors.ForEach(e => _logger.LogWarning("- {ErrorMessage}", e));
        }
    }

    private async Task SaveToDatabaseAsync(List<ProductCategoryDto> validDtos)
    {
        var categoryCache = new Dictionary<string, Category>(StringComparer.InvariantCultureIgnoreCase);
        var categoriesToAdd = new List<Category>();
        var productsToAdd = new List<Product>();

        foreach (var dto in validDtos)
        {
            var category = await GetOrCreateCategoryAsync(dto, categoryCache, categoriesToAdd);
            var product = new Product { Name = dto.ProductName.Trim(), Code = dto.ProductCode.Trim(), Category = category };
            productsToAdd.Add(product);
        }

        await PersistDataAsync(categoriesToAdd, productsToAdd);
    }

    private async Task<Category> GetOrCreateCategoryAsync(ProductCategoryDto dto, Dictionary<string, Category> categoryCache, List<Category> categoriesToAdd)
    {
        var categoryCodeNorm = dto.CategoryCode.Trim().ToLowerInvariant();
        if (!categoryCache.TryGetValue(categoryCodeNorm, out var category))
        {
            category = (await _categoryRepository.FindAsync(c => c.Code.ToLower() == categoryCodeNorm)).FirstOrDefault()
                       ?? new Category { Name = dto.CategoryName.Trim(), Code = dto.CategoryCode.Trim() };
            categoriesToAdd.Add(category);
            categoryCache[categoryCodeNorm] = category;
        }
        return category;
    }

    private async Task PersistDataAsync(List<Category> categoriesToAdd, List<Product> productsToAdd)
    {
        try
        {
            if (categoriesToAdd.Any()) await _categoryRepository.AddRangeAsync(categoriesToAdd);
            if (productsToAdd.Any()) await _productRepository.AddRangeAsync(productsToAdd);
            if (categoriesToAdd.Any() || productsToAdd.Any())
            {
                var changes = await _categoryRepository.SaveChangesAsync();
                
                _logger.LogInformation("{ChangesCount} changes saved to the database.", changes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database save.");
        }
    }
}