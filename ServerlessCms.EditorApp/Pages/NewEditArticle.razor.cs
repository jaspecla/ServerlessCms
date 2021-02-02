using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using ServerlessCms.DTO;
using ServerlessCms.EditorApp.Services;
using ServerlessCms.EditorApp.Shared;
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
    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject]
    protected ImageService ImageService { get; set; }

    public string PageTitle { get; set; } = "New Article";
    public Article Article { get; set; } = new Article();

    protected RichTextEditor RichTextEditor;

    [Parameter]
    public string Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (!string.IsNullOrEmpty(Id))
      {
        Article = await ArticleService.GetArticleById(Id);
        PageTitle = "Edit Article";
      }
      else
      {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        Article.AuthorName = user.Identity.Name;
        
      }
    }

    private async Task OnUploadImageChanged(InputFileChangeEventArgs eventArgs)
    {
      var file = eventArgs.File;

      // TODO: Ensure article ID is not null yet
      var sasUri = await ImageService.GetSasUriForImage(Article.Id, file.Name);
      await ImageService.UploadImageWithSasUri(file.OpenReadStream(file.Size), sasUri);
    }

    private async Task OnSaveArticleClicked()
    {

      Article.Content = await RichTextEditor.GetContentAsync();

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

    private void OnDiscardClicked()
    {
      NavigationManager.NavigateTo("articles");
    }
  }
}
