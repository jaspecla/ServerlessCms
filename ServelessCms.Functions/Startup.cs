using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServerlessCms.Data;
using System;

[assembly: FunctionsStartup(typeof(ServerlessCms.Functions.Startup))]

namespace ServerlessCms.Functions
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {

      builder.Services.AddSingleton<CosmosCmsDb>((s) => {
        var cosmosDbConnectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString", EnvironmentVariableTarget.Process);

        var clientOptions = new CosmosClientOptions();
        clientOptions.SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
        var dbClient = new CosmosClient(cosmosDbConnectionString, clientOptions);

        var db = new CosmosCmsDb(dbClient, "CMS", "Articles");

        return db;
      });

    }
  }
}