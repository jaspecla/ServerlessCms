namespace ServerlessCms.Data
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.Azure.Cosmos;
  using Microsoft.Azure.Cosmos.Fluent;

  public class CosmosCmsDb : ICmsDb
  {
    private Container _container;

    public CosmosCmsDb(
        CosmosClient dbClient,
        string databaseName,
        string containerName)
    {
      this._container = dbClient.GetContainer(databaseName, containerName);
    }

    public async Task AddArticleAsync(Article article)
    {
      await this._container.CreateItemAsync<Article>(article, new PartitionKey(article.Id));
    }

    public async Task DeleteArticleAsync(string id)
    {
      await this._container.DeleteItemAsync<Article>(id, new PartitionKey(id));
    }

    public async Task<Article> GetArticleAsync(string id)
    {
      try
      {
        ItemResponse<Article> response = await this._container.ReadItemAsync<Article>(id, new PartitionKey(id));
        return response.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return null;
      }

    }

    public async Task<IEnumerable<Article>> GetArticlesAsync(string queryString)
    {
      var query = this._container.GetItemQueryIterator<Article>(new QueryDefinition(queryString));
      List<Article> results = new List<Article>();
      while (query.HasMoreResults)
      {
        var response = await query.ReadNextAsync();

        results.AddRange(response.ToList());
      }

      return results;
    }

    public async Task UpdateArticleAsync(string id, Article article)
    {
      await this._container.UpsertItemAsync<Article>(article, new PartitionKey(id));
    }
  }
}