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
using ServerlessCms.DTO;
using System.Web.Http;
using ServerlessCms.Functions.Auth;

namespace ServerlessCms.Functions
{
  public class UpdateArticle
  {
    private readonly CosmosArticleDb CmsDb;
    private readonly HttpRequestAuthenticator Authenticator;

    public UpdateArticle(HttpRequestAuthenticator authenticator, CosmosArticleDb db)
    {
      Authenticator = authenticator;
      CmsDb = db;
    }

    [FunctionName("UpdateArticle")]
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

      log.LogInformation($"Update article called with data: {requestBody}");

      Article articleToUpdate = JsonConvert.DeserializeObject<Article>(requestBody);
      if (articleToUpdate == null)
      {
        log.LogError($"Invalid article format: {requestBody}");
        return new BadRequestObjectResult("Invalid article format.");
      }

      if (string.IsNullOrEmpty(articleToUpdate.Id))
      {
        log.LogError($"UpdateArticle called without article ID.");
        return new BadRequestObjectResult("Article ID is required.");
      }

      articleToUpdate.ModificationDate = DateTime.Now;

      try
      {
        await CmsDb.UpdateArticleAsync(articleToUpdate.Id, articleToUpdate);
      }
      catch (Exception ex)
      {
        log.LogError($"Error updating article: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation($"Successfully updated article with id {articleToUpdate.Id}");
      return new OkObjectResult(articleToUpdate);
    }
  }
}
