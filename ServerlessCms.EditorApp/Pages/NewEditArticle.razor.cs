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

    public Article Article { get; set; } = new Article();

    [Parameter]
    public string Id { get; set; }

    protected override Task OnInitializedAsync()
    {
      return base.OnInitializedAsync();
    }

  }
}
