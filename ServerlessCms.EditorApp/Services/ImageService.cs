using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ServerlessCms.EditorApp.Services
{
  public class ImageService
  {
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;


    public ImageService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _tokenService = tokenService;
      _configuration = configuration;
    }

    public async Task<string> GetSasUriForImage(string articleId, string fileName)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/GetImageUploadSasToken?articleId={articleId}&fileName={fileName}";
      var sasUri = await _httpClient.GetStringAsync(uri);

      return sasUri;

    }

    public async Task UploadImageWithSasUri(Stream fileStream, string sasUri)
    {
      var blobClient = new BlobClient(new Uri(sasUri));

      try
      {
        await blobClient.UploadAsync(fileStream);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Could not upload file: {e.Message}");
        throw;
      }

    }
  }
}
