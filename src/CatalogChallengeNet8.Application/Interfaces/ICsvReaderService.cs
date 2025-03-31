
namespace CatalogChallengeNet8.Application.Interfaces
{
    public interface ICsvReaderService
    {
        List<T> ReadCsv<T>(string filePath, char delimiter = ',', bool hasHeaderRecord = false) where T : class;
    }
}