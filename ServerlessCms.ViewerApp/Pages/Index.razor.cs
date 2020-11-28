using Microsoft.AspNetCore.Components;
using ServerlessCms.DTO;
using ServerlessCms.ViewerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.ViewerApp.Pages
{
  public partial class Index
  {
    [Inject]
    protected PublishedArticleService ArticleService { get; set; }
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    private IEnumerable<Article> ArticleCollection { get; set; }

    protected override async Task OnInitializedAsync()
    {
      ArticleCollection = await ArticleService.GetPublishedArticles();
    }

  }
}
