using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessCms.Data;
using Microsoft.Azure.Cosmos;
using System.Web.Http;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ServelessCms.Functions
{
  public static class CreateArticle
  {
    [FunctionName("CreateArticle")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      log.LogInformation($"Create article called with data: {requestBody}");

      Article newArticle = JsonConvert.DeserializeObject<Article>(requestBody);
      if (newArticle == null)
      {
        log.LogError($"Invalid article format: {requestBody}");
        return new BadRequestObjectResult("Invalid article format.");
      }

      newArticle.Id = Guid.NewGuid().ToString();
      newArticle.CreationDate = DateTime.Now;

      var cosmosDbConnectionString = System.Environment.GetEnvironmentVariable("CosmosDbConnectionString", EnvironmentVariableTarget.Process);

      var clientOptions = new CosmosClientOptions();
      clientOptions.SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
      var dbClient = new CosmosClient(cosmosDbConnectionString, clientOptions);

      var db = new CosmosCmsDb(dbClient, "CMS", "Articles");

      try
      {
        await db.AddArticleAsync(newArticle);
      }
      catch (Exception ex)
      {
        log.LogError($"Error loading article: {ex.Message}");
        return new InternalServerErrorResult();
      }

      return new OkObjectResult(newArticle);
    }
  }
}
