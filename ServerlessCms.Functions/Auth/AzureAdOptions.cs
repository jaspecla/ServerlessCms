using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessCms.Functions.Auth
{
  public class AzureAdOptions
  {
    public string Instance { get; set; }
    public string ClientId { get; set; }
    public string Tenant { get; set; }
    public string TenantId { get; set; }
    public string Audience { get; set; }

  }
}
