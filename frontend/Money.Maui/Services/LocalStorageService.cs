using Money.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Money.Maui.Services
{
    public class LocalStorageService : ILocalStorage
    {
        public async Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            string? value = await SecureStorage.Default.GetAsync(key);

            if(value == null)
            {
                return default(T?);
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            string stringValue = JsonSerializer.Serialize(value);

            await SecureStorage.Default.SetAsync(key, stringValue);
        }

        public async Task RemoveItemsAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            foreach (string key in keys)
            {
                await Task.Run(() =>
                {
                    SecureStorage.Default.Remove(key);
                });
            }
        }
    }
}
