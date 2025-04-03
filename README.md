[![.NET](https://github.com/luke92/catalog-api-net8/actions/workflows/ci.yml/badge.svg)](https://github.com/luke92/catalog-api-net8/actions/workflows/ci.yml)

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

### Mount Database in Docker
`docker-compose up -d`
- After that remember change appsettings.json in Project Infrastructure to run migrations over that database

## Start Import Process
`dotnet run --project src\CatalogChallengeNet8.DataImporter "C:\path\to\TestExampleFile.csv"`
- Remember change appsettings.json in Project DataImporter if you want to import the data into de docker database

## Run API
`dotnet run --project src\CatalogChallengeNet8.API`
- In Visual studio just run the http server of that project
- This should be open the url http://localhost:5077/swagger/index.html
- Remember modify your `appsettings.json` if you need

### Example of call endpoint
We can get all products using pagination (page and pageSize) and filter by category code and/or product code
```
curl -X 'GET' \
  'http://localhost:5077/api/products?page=1&pageSize=10&categoryCode=ELEC&productCode=LAPTOP01' \
  -H 'accept: text/plain'
```
And we can sort the results

```
curl -X 'GET' \
  'http://localhost:5077/api/products?page=1&pageSize=10&sortBy=categorycode&sortOrder=desc' \
  -H 'accept: text/plain'
```

### Swagger Preview
![image](https://github.com/user-attachments/assets/7404db65-9a37-4958-82a1-784e383de5a5)
![image](https://github.com/user-attachments/assets/3fe22c2f-5f07-4890-84fe-deeb07d880c0)
