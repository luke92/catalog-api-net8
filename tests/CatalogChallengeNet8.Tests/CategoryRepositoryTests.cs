using Microsoft.EntityFrameworkCore;
using CatalogChallengeNet8.Domain.Entities;
using CatalogChallengeNet8.Infrastructure.Data;
using CatalogChallengeNet8.Infrastructure.Repositories;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task AddCategoriesAsync_ValidCategories_StoresInDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var repository = new Repository<Category>(context);

            var categories = new List<Category>
            {
                new Category { Name = "Test Category", Code = "C001"}
            };

            // Act
            await repository.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new ApplicationDbContext(options))
        {
            Assert.Equal(1, await context.Categories.CountAsync());
        }
    }
}
