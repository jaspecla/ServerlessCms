using Blazored.TextEditor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.EditorApp.Shared
{
  public partial class RichTextEditor
  {
    BlazoredTextEditor QuillHtml;
    [Parameter]
    public string Content { get; set; }

    protected async override Task OnParametersSetAsync()
    {
      if (QuillHtml != null)
      {
        await QuillHtml.LoadHTMLContent(Content);
      }
    }

    public async Task<string> GetContentAsync()
    {
      return await QuillHtml.GetHTML();
    }
  }
}
