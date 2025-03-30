# catalog-api-net8
Challenge for create database and api of a catalog of products using NET 8

## Create database
Run Migration files for create the database
- First Modify the value of the property `DefaultConnection` in the file `src/CatalogChallengeNet8.Infrastructure/appsettings.json` to point with your new database
- After that run the migrations
```
dotnet ef migrations add InitialCreate --project src/CatalogChallengeNet8.Infrastructure --startup-project src/CatalogChallengeNet8.Infrastructure
dotnet ef database update --project src/CatalogChallengeNet8.Infrastructure --startup-project src/CatalogChallengeNet8.Infrastructure
```

