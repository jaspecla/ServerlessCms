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
using Azure.Messaging.ServiceBus;

namespace ServerlessCms.Functions
{
  public class AddCommentToArticle
  {
    private readonly CosmosArticleDb CmsDb;

    public AddCommentToArticle(CosmosArticleDb db)
    {
      CmsDb = db;
    }

    [FunctionName("AddCommentToArticle")]
    public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
    {

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      log.LogInformation($"Add comment to article called with data: {requestBody}");

      Comment userComment = JsonConvert.DeserializeObject<Comment>(requestBody);
     
      if (userComment == null)
      {
        log.LogError($"Invalid comment format: {requestBody}");
        return new BadRequestObjectResult("Invalid comment format.");
      }

      if (string.IsNullOrEmpty(userComment.ParentArticleId))
      {
        log.LogError($"AddCommentToArticle called without parent article ID.");
        return new BadRequestObjectResult("Parent Article ID is required.");
      }

      var articleForComment = await CmsDb.GetArticleAsync(userComment.ParentArticleId);
      if (articleForComment == null)
      {
        log.LogError($"AddCommentToArticle called, but could not find article with id {userComment.ParentArticleId}");
        return new NotFoundResult();
      }

      userComment.Id = Guid.NewGuid().ToString();
      userComment.IsPublished = false;
      userComment.CreationDate = DateTime.Now;

      var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
      var contentModerationQueueName = Environment.GetEnvironmentVariable("ContentModerationQueueName");

      await using (var client = new ServiceBusClient(connectionString))
      {
        var sender = client.CreateSender(contentModerationQueueName);
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(userComment));

        await sender.SendMessageAsync(message);
        log.LogInformation($"Sent new comment with id {userComment.Id} to moderation queue.");
      }

      return new OkObjectResult(userComment);
    }
  }
}
