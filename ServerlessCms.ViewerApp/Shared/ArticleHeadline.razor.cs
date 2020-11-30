using Microsoft.AspNetCore.Components;
using ServerlessCms.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.ViewerApp.Shared
{
  public partial class ArticleHeadline
  {
    [Parameter]
    public Article MyArticle { get; set; }

  }
}
