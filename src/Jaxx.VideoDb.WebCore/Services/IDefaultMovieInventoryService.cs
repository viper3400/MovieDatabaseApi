using Jaxx.VideoDb.WebCore.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public interface IDefaultMovieInventoryService
    {
        Task<InventoryResource> CreateInventory(string name, CancellationToken ct);
        Task<bool> DeleteInventory(int id, CancellationToken ct);
        InventoryResource GetInventory(int id);
    }
}