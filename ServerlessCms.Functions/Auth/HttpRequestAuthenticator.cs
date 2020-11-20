using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCms.Functions.Auth
{
  public static class HttpRequestAuthenticator
  {
    public static async Task<bool> AuthenticateRequestForScope(HttpRequest req, string scope, ILogger log)
    {
      var accessToken = GetAccessToken(req);
      if (accessToken == null)
      {
        return false;
      }

      var claimsPrincipal = await ValidateAccessToken(accessToken, log);
      if (claimsPrincipal == null)
      {
        return false;
      }

      var scopeClaim = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
      if (scopeClaim == null)
      {
        return false;
      }

      var scopes = scopeClaim.Value.Split();

      foreach (var scopeInClaim in scopes)
      {
        if (scopeInClaim == scope)
        {
          return true;
        }
      }

      return false;

    }

    private static string GetAccessToken(HttpRequest req)
    {
      var authorizationHeader = req.Headers?["Authorization"];
      string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
      if (parts.Length == 2 && parts[0].Equals("Bearer"))
        return parts[1];
      return null;
    }

    private static async Task<ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log)
    {
      var audience = Environment.GetEnvironmentVariable("AzureAdAudience");
      var clientID = Environment.GetEnvironmentVariable("AzureAdClientId");
      var tenant = Environment.GetEnvironmentVariable("AzureAdTenant");
      var tenantid = Environment.GetEnvironmentVariable("AzureAdTenantId");
      var aadInstance = "https://login.microsoftonline.com/{0}/v2.0";
      var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
      var validIssuers = new List<string>()
            {
                $"https://login.microsoftonline.com/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0",
                $"https://login.windows.net/{tenant}/",
                $"https://login.microsoft.com/{tenant}/",
                $"https://sts.windows.net/{tenantid}/"
            };

      // Debugging purposes only, set this to false for production
      Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

      ConfigurationManager<OpenIdConnectConfiguration> configManager =
          new ConfigurationManager<OpenIdConnectConfiguration>(
              $"{authority}/.well-known/openid-configuration",
              new OpenIdConnectConfigurationRetriever());

      OpenIdConnectConfiguration config = null;
      config = await configManager.GetConfigurationAsync();

      ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

      // Initialize the token validation parameters
      TokenValidationParameters validationParameters = new TokenValidationParameters
      {
        // App Id URI and AppId of this service application are both valid audiences.
        ValidAudiences = new[] { audience, clientID },

        // Support Azure AD V1 and V2 endpoints.
        ValidIssuers = validIssuers,
        IssuerSigningKeys = config.SigningKeys
      };

      try
      {
        SecurityToken securityToken;
        var claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);
        return claimsPrincipal;
      }
      catch (Exception ex)
      {
        log.LogError(ex.ToString());
      }

      return null;
    }
  }
}
