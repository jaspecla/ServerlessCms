using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessCms.EditorApp.Services
{
  public class TokenService
  {
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly string EditScope = "api://886f6cfd-cac3-49db-9454-391dfa053be9/CMS.Articles.Edit";
    private readonly string ReadScope = "api://886f6cfd-cac3-49db-9454-391dfa053be9/CMS.Articles.Read";

    public TokenService(IAccessTokenProvider tokenProvider)
    {
      _tokenProvider = tokenProvider;
    }
    public async Task<AccessToken> GetToken()
    {
      var token = await GetTokenForScopes(new[] { EditScope, ReadScope });
      return token;
    }

    private async Task<AccessToken> GetTokenForScopes(string[] scopes)
    {
      var tokenResult = await _tokenProvider.RequestAccessToken(
        new AccessTokenRequestOptions
        {
          Scopes = scopes
        }
      );

      if (tokenResult.TryGetToken(out var token))
      {
        return token;
      }
      else
      {
        return null;
      }

    }
  }
}
