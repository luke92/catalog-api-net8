using CatalogChallengeNet8.Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CatalogChallengeNet8.Application.Services
{
    public class CsvReaderService : ICsvReaderService
    {
        public List<T> ReadCsv<T>(string filePath, char delimiter = ',', bool hasHeaderRecord = false) where T : class
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = delimiter.ToString(),
                    HasHeaderRecord = hasHeaderRecord,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim,
                    PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<T>().ToList();
                    return records;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found '{filePath}'.");
                return new List<T>();
            }
            catch (HeaderValidationException ex)
            {
                Console.WriteLine($"Header error in the CSV: {ex.Message}. Make sure the DTO properties ({typeof(T).Name}) match the CSV headers.");
                return new List<T>();
            }
            catch (Exception ex) // Catches other exceptions (format, permissions, etc.)
            {
                Console.WriteLine($"Unexpected error while reading the CSV file '{filePath}'. Details: {ex.Message}");
                return new List<T>();
            }
        }
    }
}