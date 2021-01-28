using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessCms.Functions.Auth;
using Azure.Storage.Blobs;
using Azure.Identity;
using ServerlessCms.Data;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Models;

namespace ServerlessCms.Functions
{
  public class GetImageUploadSasToken
  {
    private readonly HttpRequestAuthenticator Authenticator;
    private readonly CosmosArticleDb CmsDb;

    public GetImageUploadSasToken(HttpRequestAuthenticator authenticator, CosmosArticleDb cmsDb)
    {
      Authenticator = authenticator;
      CmsDb = cmsDb;
    }

    [FunctionName("GetImageUploadSasToken")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      var isAuthorized = await Authenticator.AuthenticateRequestForScopeAndRole(req, "CMS.Articles.Edit", "Articles.Write", log);
      if (!isAuthorized)
      {
        return new UnauthorizedResult();
      }

      var articleId = req.Query["articleId"];
      if (string.IsNullOrEmpty(articleId))
      {
        log.LogError("GetImageUploadSasToken called without article ID");
        return new BadRequestObjectResult("articleId required");
      }

      var fileName = req.Query["fileName"];
      if (string.IsNullOrEmpty(articleId))
      {
        log.LogError("GetImageUploadSasToken called without file name");
        return new BadRequestObjectResult("fileName required");
      }


      var article = await CmsDb.GetArticleAsync(articleId);
      if (article == null)
      {
        log.LogError($"GetImageUploadSasToken called with unknown article ID: {articleId}");
        return new NotFoundObjectResult($"Article ID {articleId} not found.");
      }

      log.LogInformation($"Getting SAS token for image upload {fileName} to article {articleId}.");

      var storageAccountName = Environment.GetEnvironmentVariable("ImageStorageAccountName");
      var blobEndpoint = $"https://{storageAccountName}.blob.core.windows.net";

      var accountClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());


      UserDelegationKey userDelegationKey = null;

      try
      {
        userDelegationKey = await accountClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(30));
      }
      catch (Exception ex)
      {
        log.LogError(ex.Message);
        throw;
      }

      var sasBuilder = new BlobSasBuilder()
      {
        BlobContainerName = Environment.GetEnvironmentVariable("ImageStorageBlobContainerName"),
        BlobName = $"{articleId}/{fileName}",
        Resource = "b",
        StartsOn = DateTimeOffset.UtcNow,
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
      };

      sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

      var blobUriBuilder = new BlobUriBuilder(accountClient.Uri)
      {
        Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, accountClient.AccountName)
      };

      log.LogInformation($"Successfully created SAS token for image upload {fileName} to article {articleId}");

      return new OkObjectResult(blobUriBuilder.ToString());
    }
  }
}
