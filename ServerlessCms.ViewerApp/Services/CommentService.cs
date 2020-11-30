using Microsoft.Extensions.Configuration;
using ServerlessCms.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerlessCms.ViewerApp.Services
{
  public class CommentService
  {
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _serializerOptions;

    public CommentService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _configuration = configuration;

      _serializerOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
    }

    public async Task PostComment(Comment comment)
    {
      var uri = $"{_configuration["articleBaseUrl"]}api/AddCommentToArticle";
      var postedCommentResponse = await _httpClient.PostAsJsonAsync<Comment>(uri, comment);
    }


  }
}
