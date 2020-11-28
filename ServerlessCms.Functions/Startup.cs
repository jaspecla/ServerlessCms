using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

      var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;

      var config = new ConfigurationBuilder()
        .SetBasePath(executionContextOptions.AppDirectory)
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", false)
        .Build();

      builder.Services.Configure<AzureAdOptions>(config.GetSection("AzureAd"));

      builder.Services.AddOptions();

    }
  }
}