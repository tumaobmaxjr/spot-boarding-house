using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpotBoardingHouse.BlazorUI.Services;

namespace SpotBoardingHouse.BlazorUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            var apiBaseUrl = "https://localhost:7289/";

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<TokenStorageService>();
            builder.Services.AddScoped<ApiAuthMessageHandler>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            builder.Services.AddScoped<AuthService>();

            builder.Services.AddHttpClient("Api.Public", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });
            builder.Services.AddHttpClient("Api.Auth", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }).AddHttpMessageHandler<ApiAuthMessageHandler>();
            await builder.Build().RunAsync();
        }
    }
}
