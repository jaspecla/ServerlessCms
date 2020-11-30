using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessCms.DTO
{
  public class Comment
  {
    public string Id { get; set; }
    public string ParentArticleId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime PublicationDate { get; set; }
    public bool IsPublished { get; set; }
    public string Author { get; set; }
    public string Content { get; set; }
  }
}
