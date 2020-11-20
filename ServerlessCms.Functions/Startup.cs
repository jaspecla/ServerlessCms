using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerlessCms.Data;
using ServerlessCms.Functions.Auth;
using System;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(ServerlessCms.Functions.Startup))]

namespace ServerlessCms.Functions
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {

      builder.Services.AddSingleton<CosmosArticleDb>((s) => {
        var cosmosDbConnectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString", EnvironmentVariableTarget.Process);

        var clientOptions = new CosmosClientOptions();
        clientOptions.SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
        var dbClient = new CosmosClient(cosmosDbConnectionString, clientOptions);

        var db = new CosmosArticleDb(dbClient, "CMS", "Articles");
        return db;
      });

      builder.Services.AddSingleton<HttpRequestAuthenticator>();

      var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .AddEnvironmentVariables()
        .Build();

      builder.Services.Configure<AzureAdOptions>(config.GetSection("AzureAd"));

      builder.Services.AddOptions();

    }
  }
}