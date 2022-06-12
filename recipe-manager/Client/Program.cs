global using Microsoft.AspNetCore.Components.Authorization;
global using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using recipe_manager.Client;
using recipe_manager.Client.Providers;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthStateProvider>();

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminsOnly", policy => policy.RequireClaim("Admin"));
});
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
