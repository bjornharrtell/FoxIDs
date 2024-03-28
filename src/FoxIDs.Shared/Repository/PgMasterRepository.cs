using ITfoxtec.Identity;
using FoxIDs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using FoxIDs.Infrastructure;
using System.Collections.Generic;
using System.Linq.Expressions;
using Npgsql.DocumentDB;
using Microsoft.Extensions.DependencyInjection;

namespace FoxIDs.Repository;

public class PgMasterRepository(TelemetryLogger logger, [FromKeyedServices("master")] NpgsqlDocumentDB db) : IMasterRepository
{
    public async Task<bool> ExistsAsync<T>(string id) where T : MasterDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));
        //var partitionId = id.IdToMasterPartitionId();
        return await db.ExistsAsync(id);
    }

    public async Task<int> CountAsync<T>(Expression<Func<T, bool>> whereQuery = null) where T : MasterDocument
    {
        //var partitionId = IdToMasterPartitionId<T>();
        //var orderedQueryable = GetQueryAsync<T>(partitionId);
        //var query = (whereQuery == null) ? orderedQueryable : orderedQueryable.Where(whereQuery);
        throw new Exception("Not implemented yet");
    }

    private string IdToMasterPartitionId<T>() where T : MasterDocument
    {
        if (typeof(T) == typeof(Plan))
        {
            return Plan.PartitionIdFormat(new MasterDocument.IdKey());
        }
        else if (typeof(T) == typeof(RiskPassword))
        {
            return RiskPassword.PartitionIdFormat(new MasterDocument.IdKey());
        }
        else
        {
            return MasterDocument.PartitionIdFormat(new MasterDocument.IdKey());
        }
    }

    public async Task<T> GetAsync<T>(string id, bool required = true) where T : MasterDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));
        //var partitionId = id.IdToMasterPartitionId();
        return await db.GetAsync<T>(id);
    }

    public async Task<HashSet<T>> GetListAsync<T>(Expression<Func<T, bool>> whereQuery = null, int maxItemCount = 50) where T : MasterDocument
    {
        //var partitionId = IdToMasterPartitionId<T>();
        //var query = GetQueryAsync<T>(partitionId, maxItemCount: maxItemCount);
        //var setIterator = (whereQuery == null) ? query.ToFeedIterator() : query.Where(whereQuery).ToFeedIterator();
        throw new Exception("Not implemented yet");
    }

    public async Task CreateAsync<T>(T item) where T : MasterDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.PartitionId = item.Id.IdToMasterPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task UpdateAsync<T>(T item) where T : MasterDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.PartitionId = item.Id.IdToMasterPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task SaveAsync<T>(T item) where T : MasterDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        item.PartitionId = item.Id.IdToMasterPartitionId();
        item.SetDataType();
        await item.ValidateObjectAsync();

        await db.SetAsync(item.Id, item);
    }

    public async Task<T> DeleteAsync<T>(string id) where T : MasterDocument
    {
        if (id.IsNullOrWhiteSpace()) new ArgumentNullException(nameof(id));

        //var partitionId = id.IdToMasterPartitionId();
        var item = await db.GetAsync<T>(id);
        await db.RemoveAsync(id);
        return item;
    }

    public async Task DeleteAsync<T>(T item) where T : MasterDocument
    {
        if (item == null) new ArgumentNullException(nameof(item));
        if (item.Id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(item.Id), item.GetType().Name);

        //var partitionId = item.Id.IdToMasterPartitionId();

        await db.RemoveAsync(item.Id);
    }

    public async Task SaveBulkAsync<T>(List<T> items) where T : MasterDocument
    {
        if (items?.Count <= 0) new ArgumentNullException(nameof(items));
        var firstItem = items.First();
        if (firstItem.Id.IsNullOrEmpty()) throw new ArgumentNullException($"First item {nameof(firstItem.Id)}.", items.GetType().Name);

        var partitionId = firstItem.Id.IdToMasterPartitionId();
        foreach (var item in items)
        {
            item.PartitionId = partitionId;
            item.SetDataType();
            await item.ValidateObjectAsync();
            await db.SetAsync(item.Id, item);
        }
    }

    public async Task DeleteBulkAsync<T>(List<string> ids) where T : MasterDocument
    {
        if (ids?.Count <= 0) new ArgumentNullException(nameof(ids));
        var firstId = ids.First();
        if (firstId.IsNullOrEmpty()) throw new ArgumentNullException($"First id {nameof(firstId)}.", ids.GetType().Name);

        //var partitionId = firstId.IdToMasterPartitionId();
        foreach (var id in ids)
        {
            await db.RemoveAsync(id);
        }
    }
}
