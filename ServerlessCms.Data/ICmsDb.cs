using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessCms.Data
{
  public interface ICmsDb
  {
    Task AddArticleAsync(Article item);
    Task DeleteArticleAsync(string id);
    Task<Article> GetArticleAsync(string id);
    Task<IEnumerable<Article>> GetArticlesAsync(string queryString);
    Task UpdateArticleAsync(string id, Article item);

  }
}