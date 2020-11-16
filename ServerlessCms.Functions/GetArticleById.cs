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
using System.Web.Http;
using ServerlessCms.DTO;

namespace ServelessCms.Functions
{
  public class GetArticleById
  {
    public readonly CosmosArticleDb CmsDb;

    public GetArticleById(CosmosArticleDb db)
    {
      CmsDb = db;
    }

    [FunctionName("GetArticleById")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      string id = req.Query["id"];

      if (string.IsNullOrEmpty(id))
      {
        log.LogError("No id provided to GetArticleById Function.");
        return new BadRequestObjectResult("GetArticleById requires an id.");
      }

      log.LogInformation($"Getting articles with id {id}");

      Article article;

      try
      {
        article = await CmsDb.GetArticleAsync(id);
      }
      catch (Exception ex)
      {
        log.LogError($"Error loading article with id {id}: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation($"Successfully retrieved article with id: {id}");

      return new OkObjectResult(article);
    }
  }
}
