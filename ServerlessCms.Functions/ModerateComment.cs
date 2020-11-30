using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessCms.Data;
using ServerlessCms.DTO;

namespace ServerlessCms.Functions
{
  public class ModerateComment
  {
    private readonly CosmosArticleDb CmsDb;

    public ModerateComment(CosmosArticleDb cmsDb)
    {
      CmsDb = cmsDb;
    }

    [FunctionName("ModerateComment")]
    public async Task Run([ServiceBusTrigger("contentmoderation", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
    {
      log.LogInformation($"ModerateComment function processed: {myQueueItem}");

      var comment = JsonConvert.DeserializeObject<Comment>(myQueueItem);

      if (comment == null)
      {
        log.LogError($"Could not deserialize comment: {myQueueItem}");
        return;
      }

      var commentText = comment.Content;
      commentText = commentText.Replace(Environment.NewLine, " ");
      var commentTextBytes = Encoding.UTF8.GetBytes(commentText);
      var commentTextStream = new MemoryStream(commentTextBytes);

      var subscriptionKey = Environment.GetEnvironmentVariable("ContentModeratorSubscriptionKey");
      var contentModeratorClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(subscriptionKey));

      contentModeratorClient.Endpoint = Environment.GetEnvironmentVariable("ContentModeratorEndpoint");


      using (contentModeratorClient)
      {
        var result = await contentModeratorClient.TextModeration.ScreenTextAsync(
          "text/plain", 
          commentTextStream, 
          language: "eng", 
          autocorrect: false, 
          pII: true, 
          listId: null, 
          classify: true);

        if (result.Classification.ReviewRecommended ?? false)
        {
          log.LogInformation($"Content was NOT approved for publication: {commentText}");
          return;
        }
        else
        {
          log.LogInformation($"Publishing comment {comment.Id} on article {comment.ParentArticleId}");
        }

      }

      var article = await CmsDb.GetArticleAsync(comment.ParentArticleId);
      if (article == null)
      {
        log.LogError($"Could not find article with id {comment.ParentArticleId}");
        return;
      }

      comment.IsPublished = true;
      comment.PublicationDate = DateTime.Now;

      List<Comment> comments;
      if (article.Comments == null)
      {
        comments = new List<Comment>();
      }
      else
      {
        comments = article.Comments.ToList();
      }

      comments.Add(comment);
      article.Comments = comments;

      try
      {
        await CmsDb.UpdateArticleAsync(article.Id, article);
      }
      catch (Exception ex)
      {
        log.LogError($"Error updating article {article.Id} with comment: {ex.Message}");
        return;
      }

    }
  }
}
