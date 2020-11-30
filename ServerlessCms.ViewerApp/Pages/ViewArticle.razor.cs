using Microsoft.AspNetCore.Components;
using ServerlessCms.DTO;
using ServerlessCms.ViewerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.ViewerApp.Pages
{
  public partial class ViewArticle
  {
    [Parameter]
    public string Id { get; set; }

    [Inject]
    public PublishedArticleService ArticleService { get; set; }

    protected Article MyArticle { get; set; }

    protected override async Task OnInitializedAsync()
    {
      MyArticle = await ArticleService.GetPublishedArticleById(Id);
    }
  }
}
