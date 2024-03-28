using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.DocumentDB;

namespace FoxIDs.Logic;

public class PgCacheProvider([FromKeyedServices("cache")] NpgsqlDocumentDB db) : IDistributedCacheProvider
{
    public async Task<bool> DeleteAsync(string key) =>
        await db.RemoveAsync(key);

    public async Task<bool> ExistsAsync(string key) =>
        await db.ExistsAsync(key);

    public async Task<string> GetAsync(string key) =>
        await db.GetAsync<string>(key);

    public async Task<long> GetNumberAsync(string key) =>
        await db.GetAsync<long?>(key) ?? 0;

    public async Task<bool> SetAsync(string key, string value, int lifetime)
    {
        await db.SetAsync(key, value); // TimeSpan.FromSeconds(lifetime))
        return true;
    }

    public async Task<bool> SetFlagAsync(string key, int lifetime)
    {
        return await SetAsync(key, true.ToString(), lifetime);
    }

    public async Task<long> IncrementNumberAsync(string key, int? lifetime = null)
    {
        var count = (await db.GetAsync<long?>(key) ?? 0) + 1;
        await db.SetAsync(key, count); // TimeSpan.FromSeconds(lifetime.Value)
        return count;
    }
}