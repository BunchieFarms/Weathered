using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Weathered_Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string baseAddress = "http://weathered.bryceohmer.com";
#if DEBUG
    baseAddress = "http://localhost:5085";
#endif

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddMudServices();

await builder.Build().RunAsync();
