using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Money.ApiClient;
using MudBlazor.Services;
using MudBlazor.Translations;
using NCalc.DependencyInjection;
using Xamarin.Essentials;

namespace Money.Shared
{
    public static class SharedDefinitions
    {
        public static HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };
            return handler;
        }
        public static void ConfigureHybryd(this IServiceCollection services)
        {
            services.AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.PreventDuplicates = false;
            });

            services.AddMudTranslations();
            services.AddBlazoredLocalStorage();

            services.AddLocalization();
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddNCalc();
            services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<JwtParser>();
            services.AddScoped<RefreshTokenService>();
            services.AddScoped<RefreshTokenService>();
            services.AddScoped<CategoryService>();
            services.AddTransient<RefreshTokenHandler>();

            string baseUrl = $"https://{(DeviceInfo.DeviceType == DeviceType.Virtual ? "10.0.2.2" : "localhost")}:7124/";

            services.AddHttpClient("api")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUrl))
                .ConfigurePrimaryHttpMessageHandler(GetInsecureHandler)
                .AddHttpMessageHandler<RefreshTokenHandler>();

            services.AddHttpClient("api_base")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUrl))
                .ConfigurePrimaryHttpMessageHandler(GetInsecureHandler);

            services.AddScoped(provider =>
            {
                IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("api");
            });

            services.AddScoped(provider =>
            {
                HttpClient client = provider.GetRequiredService<HttpClient>();
                MoneyClient moneyClient = new(client, Console.WriteLine);
                return moneyClient;
            });
        }
    }
}
