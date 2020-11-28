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
using System.Collections.Generic;
using ServerlessCms.DTO;
using ServerlessCms.Functions.Auth;

namespace ServelessCms.Functions
{
  public class GetPublishedArticles
  {
    private readonly CosmosArticleDb CmsDb;

    public GetPublishedArticles(CosmosArticleDb db)
    {
      CmsDb = db;
    }

    [FunctionName("GetPublishedArticles")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {

      log.LogInformation("Getting all published articles.");

      IEnumerable<Article> articleCollection;

      try
      {
        articleCollection = await CmsDb.GetArticlesAsync("SELECT * FROM Articles a WHERE a.isPublished = true ORDER BY a.publicationDate DESC");
      }
      catch (Exception ex)
      {
        log.LogError($"Error loading articles: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation("Successfully retrieved all articles.");

      return new OkObjectResult(articleCollection);
    }
  }
}
