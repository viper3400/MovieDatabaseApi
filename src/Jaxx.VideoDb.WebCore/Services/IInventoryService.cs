using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public interface IInventoryService
    {
        Task<InventoryResource> CreateInventory(string name, DateTime startTime, CancellationToken ct);
        Task<bool> DeleteInventory(int id, CancellationToken ct);
        InventoryResource GetInventory(int id);
        Task<Page<InventoryResource>> GetInventories(string nameFilter, PagingOptions pagingOptions, CancellationToken ct);
        Task<InventoryResource> FinishInventory(int id, DateTime endTime, CancellationToken ct);
        Task<Page<InventoryDataResource>> GetInventoryDataForRack(int inventoryId, string rackId, CancellationToken ct);
        Task<IEnumerable<InventoryRackModel>> GetInventoryRacks(int inventoryId, CancellationToken ct);
        Task<Page<InventoryDataResource>> SaveInventoryDataForRack(int inventoryId, string rackId, IEnumerable<InventoryDataResource> resources, CancellationToken ct);
        Task<InventoryResource> AbandonInventory(int id, DateTime abandonDate, CancellationToken ct);
    }
}