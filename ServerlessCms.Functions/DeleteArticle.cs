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
using ServerlessCms.Functions.Auth;

namespace ServelessCms.Functions
{

  public class DeleteArticle
  {
    private readonly CosmosArticleDb CmsDb;
    private readonly HttpRequestAuthenticator Authenticator;

    public DeleteArticle(HttpRequestAuthenticator authenticator, CosmosArticleDb db)
    {
      CmsDb = db;
      Authenticator = authenticator;
    }

    [FunctionName("DeleteArticle")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      var isAuthorized = await Authenticator.AuthenticateRequestForScopeAndRole(req, "CMS.Articles.Edit", "Articles.Write", log);
      if (!isAuthorized)
      {
        return new UnauthorizedResult();
      }

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      log.LogInformation($"Delete article called with data: {requestBody}");

      Article articleToDelete = JsonConvert.DeserializeObject<Article>(requestBody);
      if (articleToDelete == null)
      {
        log.LogError($"Invalid article format: {requestBody}");
        return new BadRequestObjectResult("Invalid article format.");
      }

      try
      {
        await CmsDb.DeleteArticleAsync(articleToDelete.Id);
      }
      catch (Exception ex)
      {
        log.LogError($"Error deleting article: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation($"Successfully deleted article with id {articleToDelete.Id}");
      return new OkObjectResult(articleToDelete);
    }
  }
}
