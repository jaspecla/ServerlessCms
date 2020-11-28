using ServerlessCms.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace ServerlessCms.EditorApp.Services
{
  public class ArticleService
  {

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly TokenService _tokenService;
    public ArticleService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _tokenService = tokenService;
      _configuration = configuration;

      _serializerOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
  }
    public async Task<IEnumerable<Article>> GetArticles()
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/GetArticles";
      var articles = await _httpClient.GetFromJsonAsync<Article[]>(uri);

      return articles;
    }

    public async Task<Article> GetArticleById(string id)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/GetArticleById?id={id}";
      var article = await _httpClient.GetFromJsonAsync<Article>(uri);

      return article;
    }

    public async Task CreateArticle(Article article)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/CreateArticle";
      var createdArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);
    }

    public async Task UpdateArticle(Article article)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/UpdateArticle";
      var updatedArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);
    }

    public async Task PublishArticle(Article article)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/PublishArticle";
      var updatedArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);
    }

    public async Task DeleteArticle(Article article)
    {
      var token = await _tokenService.GetToken();
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

      var uri = $"{_configuration["articleBaseUrl"]}api/DeleteArticle";
      var updatedArticleResponse = await _httpClient.PostAsJsonAsync<Article>(uri, article);

    }

  }
}
