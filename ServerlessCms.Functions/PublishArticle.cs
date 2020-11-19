using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessCms.DTO;
using ServerlessCms.Data;
using System.Web.Http;
using ServerlessCms.Functions.Auth;

namespace ServerlessCms.Functions
{
  public class PublishArticle
  {

    private readonly CosmosArticleDb CmsDb;

    public PublishArticle(CosmosArticleDb cmsDb)
    {
      CmsDb = cmsDb;
    }

    [FunctionName("PublishArticle")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      var isAuthorized = await HttpRequestAuthenticator.AuthenticateRequestForScope(req, "CMS.Articles.Edit", log);
      if (!isAuthorized)
      {
        return new UnauthorizedResult();
      }

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      log.LogInformation($"Publish article called with data: {requestBody}");

      Article articleToPublish = JsonConvert.DeserializeObject<Article>(requestBody);
      if (articleToPublish == null)
      {
        log.LogError($"Invalid article format: {requestBody}");
        return new BadRequestObjectResult("Invalid article format.");
      }

      if (string.IsNullOrEmpty(articleToPublish.Id))
      {
        log.LogError($"PublishArticle called without article ID.");
        return new BadRequestObjectResult("Article ID is required.");
      }

      articleToPublish.PublicationDate = DateTime.Now;
      articleToPublish.IsPublished = true;

      try
      {
        await CmsDb.UpdateArticleAsync(articleToPublish.Id, articleToPublish);
      }
      catch (Exception ex)
      {
        log.LogError($"Error publishing article: {ex.Message}");
        return new InternalServerErrorResult();
      }

      log.LogInformation($"Successfully published article with id {articleToPublish.Id}");
      return new OkObjectResult(articleToPublish);
    }
  }
}
