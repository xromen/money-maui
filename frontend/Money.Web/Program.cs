using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Money.Shared;
using Money.Shared.Interfaces;
using Money.Web.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IFormFactor, FormFactorService>();
builder.Services.AddScoped<ILocalStorage, LocalStorageService>();

builder.Services.ConfigureHybryd();

var app = builder.Build();

await app.RunAsync();
