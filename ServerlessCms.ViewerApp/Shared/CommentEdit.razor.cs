using Microsoft.AspNetCore.Components;
using ServerlessCms.DTO;
using ServerlessCms.ViewerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.ViewerApp.Shared
{
  public partial class CommentEdit
  {
    [Inject]
    protected CommentService CommentService { get; set; }

    [Parameter]
    public string ParentArticleId { get; set; }
    protected bool CommentWasPosted { get; set; }
    public Comment MyComment { get; set; } = new Comment();

    protected override void OnInitialized()
    {
      MyComment.ParentArticleId = ParentArticleId;
    }
    protected async Task OnPostCommentClicked()
    {
      await CommentService.PostComment(MyComment);
      CommentWasPosted = true;
    }

    protected void OnDiscardClicked()
    {
      MyComment.Author = string.Empty;
      MyComment.Content = string.Empty;
    }

    protected void OnPostAnotherCommentClicked()
    {
      MyComment = new Comment();
      MyComment.ParentArticleId = ParentArticleId;
      CommentWasPosted = false;
    }
  }
}
