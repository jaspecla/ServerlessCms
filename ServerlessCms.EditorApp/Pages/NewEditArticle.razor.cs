using Microsoft.AspNetCore.Components;
using ServerlessCms.DTO;
using ServerlessCms.EditorApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.EditorApp.Pages
{
  public partial class NewEditArticle 
  {
    [Inject]
    protected ArticleService ArticleService { get; set; }
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    public string PageTitle { get; set; } = "New Article";
    public Article Article { get; set; } = new Article();

    [Parameter]
    public string Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (!string.IsNullOrEmpty(Id))
      {
        Article = await ArticleService.GetArticleById(Id);
        PageTitle = "Edit Article";
      }
    }

    public async Task OnSaveArticleClicked()
    {
      if (string.IsNullOrEmpty(Article.Id))
      {
        Article.Id = Guid.NewGuid().ToString();
      }

      if (string.IsNullOrEmpty(Id))
      {
        await ArticleService.CreateArticle(Article);
      }
      else
      {
        await ArticleService.UpdateArticle(Article);
      }

      NavigationManager.NavigateTo("articles");
    }

    public void OnDiscardClicked()
    {
      NavigationManager.NavigateTo("articles");
    }
  }
}
