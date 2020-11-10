using System;

namespace ServerlessCms.Data
{
  public class Article
  {
    public string Id { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime PublicationDate { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string AuthorName { get; set; }
    public string Content { get; set; }
  }
}
