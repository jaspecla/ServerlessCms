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
using ServerlessCms.DTO;

namespace ServelessCms.Functions
{
  
  public class CreateArticle
  {
    private readonly CosmosArticleDb CmsDb;

    public CreateArticle(CosmosArticleDb db)
    {
      CmsDb = db;
    }

    [FunctionName("CreateArticle")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
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

      try
      {
        await CmsDb.AddArticleAsync(newArticle);
      }
      catch (Exception ex)
      {
        log.LogError($"Error loading article: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation($"Successfully created article with id {newArticle.Id}");
      return new OkObjectResult(newArticle);
    }
  }
}
