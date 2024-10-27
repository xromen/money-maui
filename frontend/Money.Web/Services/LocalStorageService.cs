using Blazored.LocalStorage;
using Money.Shared.Interfaces;
using System.Text.Json;
using Xamarin.Essentials;

namespace Money.Web.Services
{
    public class LocalStorageService(ILocalStorageService localStorage, IFormFactor formFactorService) : ILocalStorage
    {
        public async Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await localStorage.GetItemAsync<T>(key, cancellationToken);
        }

        public async Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            await localStorage.SetItemAsync(key, value, cancellationToken);
        }

        public async Task RemoveItemsAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            await localStorage.RemoveItemsAsync(keys, cancellationToken);
        }
    }
}
