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
  public class GetArticles
  {
    public readonly CosmosArticleDb CmsDb;

    public GetArticles(CosmosArticleDb db)
    {
      CmsDb = db;
    }

    [FunctionName("GetArticles")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      var isAuthorized = await HttpRequestAuthenticator.AuthenticateRequestForScope(req, "CMS.Articles.Read", log);
      if (!isAuthorized)
      {
        return new UnauthorizedResult();
      }


      log.LogInformation("Getting all articles.");

      IEnumerable<Article> articleCollection;

      try
      {
        articleCollection = await CmsDb.GetArticlesAsync("SELECT * FROM c");
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
