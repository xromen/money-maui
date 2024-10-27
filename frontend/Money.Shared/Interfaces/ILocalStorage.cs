using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Money.Shared.Interfaces
{
    public interface ILocalStorage
    {
        public Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);
        public Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default);
        public Task RemoveItemsAsync(string[] keys, CancellationToken cancellationToken = default);
    }
}
