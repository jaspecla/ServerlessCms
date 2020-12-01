using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCms.Functions.Auth
{
  public class HttpRequestAuthenticator
  {
    private readonly AzureAdOptions _azureAdOptions;

    public HttpRequestAuthenticator(IOptions<AzureAdOptions> azureAdOptionsAccessor)
    {
      _azureAdOptions = azureAdOptionsAccessor.Value;
    }

    public async Task<bool> AuthenticateRequestForScopeAndRole(HttpRequest req, string scope, string role, ILogger log)
    {
      var accessToken = GetAccessToken(req);
      if (accessToken == null)
      {
        log.LogError("Could not get access token from request.");
        return false;
      }

      var claimsPrincipal = await ValidateAccessToken(accessToken, log);
      if (claimsPrincipal == null)
      {
        log.LogError("Could not validate access token to get claims principal.");
        return false;
      }

      if (!ValidateValueInClaim(claimsPrincipal, "http://schemas.microsoft.com/identity/claims/scope", scope))
      {
        log.LogError($"Scope {scope} not found for calling application.");
        return false;
      }

      if (!ValidateValueInClaim(claimsPrincipal, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role))
      {
        log.LogError($"User {claimsPrincipal.Identity.Name} not authorized for role {role}.");
        return false;
      }

      log.LogInformation($"Scope and role claims are valid for user {claimsPrincipal.Identity.Name}.");
      return true;

    }

    private bool ValidateValueInClaim(ClaimsPrincipal claimsPrincipal, string claimName, string value)
    {
      var claims = claimsPrincipal.FindAll(claimName);

      foreach (var claim in claims)
      {
        if (claim != null && claim.Value.Split().Contains(value))
        {
          return true;
        }
      }

      return false;
    }

    private string GetAccessToken(HttpRequest req)
    {
      var authorizationHeader = req.Headers?["Authorization"];
      string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
      if (parts.Length == 2 && parts[0].Equals("Bearer"))
        return parts[1];
      return null;
    }

    private async Task<ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log)
    {
      var audience = _azureAdOptions.Audience;
      var clientID = _azureAdOptions.ClientId;
      var tenant = _azureAdOptions.Tenant;
      var tenantid = _azureAdOptions.TenantId;
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
