using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerlessCms.EditorApp;
using ServerlessCms.EditorApp.Services;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ImageService>();

builder.Services.AddMsalAuthentication(options =>
{
  builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
  options.ProviderOptions.DefaultAccessTokenScopes
    .Add("https://graph.microsoft.com/User.Read");

});

await builder.Build().RunAsync();
