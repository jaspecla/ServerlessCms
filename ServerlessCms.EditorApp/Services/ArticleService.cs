using ServerlessCms.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace ServerlessCms.EditorApp.Services
{
  public class ArticleService
  {

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _serializerOptions;
    public ArticleService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _configuration = configuration;

      _serializerOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
  }
    public async Task<IEnumerable<Article>> GetArticles()
    {
      var uri = $"{_configuration["articleBaseUrl"]}api/GetArticles";
      var articles = await _httpClient.GetFromJsonAsync<Article[]>(uri);

      return articles;
    }

    public async Task<Article> GetArticleById(string id)
    {
      var uri = $"{_configuration["articleBaseUrl"]}api/GetArticleById?id={id}";
      var article = await _httpClient.GetFromJsonAsync<Article>(uri);

      return article;
    }

    public async Task CreateArticle(Article article)
    {
      var uri = $"{_configuration["articleBaseUrl"]}api/CreateArticle";
      var createdArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);
    }

    public async Task UpdateArticle(Article article)
    {
      var uri = $"{_configuration["articleBaseUrl"]}api/UpdateArticle";
      var updatedArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);
    }

  }
}
