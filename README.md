# catalog-api-net8
Challenge for create database and api of a catalog of products using NET 8

## Requirements
- .Net SDK 8
- SQL Server LocalDB 2019 (Optional)

## Create database
Run Migration files for create the database
- First Modify the value of the property `DefaultConnection` in the file `src/CatalogChallengeNet8.Infrastructure/appsettings.json` to point with your new database
- Run `dotnet restore`
- After that run the migrations
```
dotnet ef database update --project src/CatalogChallengeNet8.Infrastructure --startup-project src/CatalogChallengeNet8.Infrastructure
```

