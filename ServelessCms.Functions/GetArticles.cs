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

namespace ServelessCms.Functions
{
  public class GetArticles
  {
    public readonly CosmosCmsDb CmsDb;

    public GetArticles(CosmosCmsDb db)
    {
      CmsDb = db;
    }

    [FunctionName("GetArticles")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("Getting all articles.");

      var articleCollection = await CmsDb.GetArticlesAsync("SELECT * FROM c");

      return new OkObjectResult(articleCollection);
    }
  }
}
