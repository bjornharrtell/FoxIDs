using ITfoxtec.Identity;
using FoxIDs.Models;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FoxIDs.Infrastructure;
using System.Collections.Generic;
using Npgsql.DocumentDB;
using Microsoft.Extensions.DependencyInjection;

namespace FoxIDs.Repository;

public class PgTenantRepository([FromKeyedServices("master")] NpgsqlDocumentDB db) : ITenantRepository
{
    public async Task<bool> ExistsAsync<T>(string id, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));
        return await db.ExistsAsync(id);
    }

    public async Task<int> CountAsync<T>(Track.IdKey idKey = null, Expression<Func<T, bool>> whereQuery = null, bool usePartitionId = true) where T : IDataDocument
    {
        //var partitionId = usePartitionId ? PartitionIdFormat<T>(idKey) : null;
        //var orderedQueryable = GetQueryAsync<T>(partitionId, usePartitionId: usePartitionId);
        //var query = (whereQuery == null) ? orderedQueryable : orderedQueryable.Where(whereQuery);

        throw new Exception("Not implemented yet");
    }

    public async Task<T> GetAsync<T>(string id, bool required = true, bool delete = false, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));

        return await db.GetAsync<T>(id);
    }

    public async Task<Tenant> GetTenantByNameAsync(string tenantName, bool required = true, TelemetryScopedLogger scopedLogger = null)
    {
        if (tenantName.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(tenantName));
        //var partitionId = Tenant.PartitionIdFormat();
        var id = await Tenant.IdFormatAsync(tenantName);
        return await db.GetAsync<Tenant>(id);
    }

    public async Task<Track> GetTrackByNameAsync(Track.IdKey idKey, bool required = true, TelemetryScopedLogger scopedLogger = null)
    {
        if (idKey == null) new ArgumentNullException(nameof(idKey));
        //var partitionId = Track.PartitionIdFormat();
        var id = await Track.IdFormatAsync(idKey);
        return await db.GetAsync<Track>(id);
    }


    public async Task<(HashSet<T> items, string continuationToken)> GetListAsync<T>(Track.IdKey idKey = null, Expression<Func<T, bool>> whereQuery = null, int maxItemCount = 50, string continuationToken = null, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        //var partitionId = PartitionIdFormat<T>(idKey);
        //var query = GetQueryAsync<T>(partitionId, maxItemCount: maxItemCount, continuationToken: continuationToken);
        //var setIterator = (whereQuery == null) ? query.ToFeedIterator() : query.Where(whereQuery).ToFeedIterator();
        throw new Exception("Not implemented yet");
    }

    public async Task CreateAsync<T>(T item, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.SetTenantPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task UpdateAsync<T>(T item, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.SetTenantPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task SaveAsync<T>(T item, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.SetTenantPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task<T> DeleteAsync<T>(string id, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));

        //var partitionId = id.IdToTenantPartitionId();

        var item = await db.GetAsync<T>(id);
        await db.RemoveAsync(id);
        return item;
    }

    public async Task<T> DeleteAsync<T>(Track.IdKey idKey, Expression<Func<T, bool>> whereQuery = null, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (idKey == null) new ArgumentNullException(nameof(idKey));
        await idKey.ValidateObjectAsync();

        //var partitionId = PartitionIdFormat<T>(idKey);
        //var query = GetQueryAsync<T>(partitionId);
        //var setIterator = (whereQuery == null) ? query.ToFeedIterator() : query.Where(whereQuery).ToFeedIterator();

        throw new Exception("Not implemented yet");
    }

    public async Task<int> DeleteListAsync<T>(Track.IdKey idKey, Expression<Func<T, bool>> whereQuery = null, TelemetryScopedLogger scopedLogger = null) where T : IDataDocument
    {
        if (idKey == null) new ArgumentNullException(nameof(idKey));
        await idKey.ValidateObjectAsync();

        //var partitionId = PartitionIdFormat<T>(idKey);
        //var query = GetQueryAsync<T>(partitionId, -1);
        //var setIterator = (whereQuery == null) ? query.ToFeedIterator() : query.Where(whereQuery).ToFeedIterator();

        throw new Exception("Not implemented yet");
    }

    private string PartitionIdFormat<T>(Track.IdKey idKey) where T : IDataDocument
    {
        if (typeof(T).Equals(typeof(Tenant)))
        {
            return Tenant.PartitionIdFormat();
        }
        else if (typeof(T).Equals(typeof(Track)))
        {
            if (idKey == null) new ArgumentNullException(nameof(idKey));
            return Track.PartitionIdFormat(idKey);
        }
        else
        {
            if (idKey == null) new ArgumentNullException(nameof(idKey));
            return DataDocument.PartitionIdFormat(idKey);
        }
    }
}

