using AutoMapper;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly VideoDbContext _context;
        private readonly ILogger<InventoryService> _logger;
        private readonly IMapper _mapper;
        private readonly MovieDataServiceOptions _options;
        private readonly IMovieDataService _movieDataService;

        public InventoryService(VideoDbContext context,
            MovieDataServiceOptions serviceOptions,
            ILogger<InventoryService> logger,
            IMapper mapper,
            IMovieDataService movieDataService)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _options = serviceOptions;
            _logger.LogDebug("New instance created.");
            _movieDataService = movieDataService;
        }

        /// <summary>
        /// Creates and starts a new inventory.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        /// <param name="ct"></param>
        /// <returns>A task with the new inventory resource.</returns>
        public async Task<InventoryResource> CreateInventory(string name, DateTime startTime, CancellationToken ct)
        {
            var inventoryEntity = new homewebbridge_inventory { name = name, starttime = startTime, endtime = null, state = (int)InventoryState.Started };
            _context.HomeWebInventory.Add(inventoryEntity);
            await _context.SaveChangesAsync(ct);
            return MapFromInventoryEntity(inventoryEntity);
        }

        public async Task<bool> DeleteInventory(int id, CancellationToken ct)
        {
            await DeleteInventoryData(id, ct);
            var inventoryEntity = GetInventoryEntityById(id);
            _context.HomeWebInventory.Remove(inventoryEntity);
            var changes = await _context.SaveChangesAsync(ct);
            if (changes == 1) return true;
            else return false;
        }

        public InventoryResource GetInventory(int id)
        {
            var inventoryEntity = GetInventoryEntityById(id);
            return MapFromInventoryEntity(inventoryEntity);
        }

        public async Task<Page<InventoryResource>> GetInventories(string nameFilter, PagingOptions pagingOptions, CancellationToken ct)
        {
            IQueryable<homewebbridge_inventory> query = _context.HomeWebInventory;

            if (!string.IsNullOrWhiteSpace(nameFilter)) query = query.Where(i => i.name.Contains(nameFilter));

            var size = await query.CountAsync(ct);

            var items = await query
                //.Include(o => o.data)
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ToListAsync(ct);

            var mappedItems = _mapper.Map<IEnumerable<InventoryResource>>(items);

            return new Page<InventoryResource>
            {
                Items = mappedItems,
                TotalSize = size
            };
        }

        public async Task<InventoryResource> FinishInventory(int id, DateTime endTime, CancellationToken ct)
        {
            var inventoryEntity = GetInventoryEntityById(id);
            if (inventoryEntity.state == (int)InventoryState.Finished) throw new NotSupportedException($"Inventory with {id} has alreay been finished and could not be finished twice.");
            inventoryEntity.endtime = endTime;
            inventoryEntity.state = (int)InventoryState.Finished;
            await _context.SaveChangesAsync(ct);
            return MapFromInventoryEntity(inventoryEntity);
        }

        private homewebbridge_inventory GetInventoryEntityById(int id)
        {
            return _context.HomeWebInventory.Where(e => e.id == id).FirstOrDefault();
        }

        private InventoryResource MapFromInventoryEntity(homewebbridge_inventory entity)
        {
            return _mapper.Map<InventoryResource>(entity);
        }

        private async Task<homewebbridge_inventorydata> CreateInventoryDataEntry(int inventoryId, int movieId, string rackid, CancellationToken ct)
        {
            var dataEntity = new homewebbridge_inventorydata { inventoryid = inventoryId, movieid = movieId, rackid = rackid, state = (int)InventoryDataState.NotChecked };
            await _context.HomeWebInventoryData.AddAsync(dataEntity, ct);
            await _context.SaveChangesAsync(ct);
            return dataEntity;
        }

        private async Task<Page<InventoryDataResource>> StartInventoryForRack(int inventoryId, string rackId, CancellationToken ct)
        {
            var movies = await _movieDataService.GetMovieDataAsync(
                null,
                new PagingOptions { Limit = 100, Offset = 0 },
                new MovieDataOptions { Search = rackId, IsDeleted = "false", UseInlineCoverImage = true, SortOrder = MovieDataSortOrder.ByDiskIdAsc }, ct);

            foreach (var movie in movies.Items)
            {
                await CreateInventoryDataEntry(inventoryId, movie.id, rackId, ct);
            }

            return await GetInventoryDataForRack(inventoryId, rackId, ct);
        }

        public async Task<Page<InventoryDataResource>> GetInventoryDataForRack(int inventoryId, string rackId, CancellationToken ct)
        {
            var query = _context.HomeWebInventoryData
                .Where(i => i.inventoryid == inventoryId && i.rackid == rackId)
                .Include(i => i.inventory);

            var items = await query.ToListAsync();

            if (items.Count() > 0)
            {
                return MapInventoryDataToPage(items);
            }
            else return await StartInventoryForRack(inventoryId, rackId, ct);
        }

        public async Task<Page<InventoryDataResource>> GetInventoryData(int inventoryId, CancellationToken ct)
        {
            var query = _context.HomeWebInventoryData
                .Where(i => i.inventoryid == inventoryId)
                .Include(i => i.inventory);

            var items = await query.ToListAsync();
            return MapInventoryDataToPage(items);
        }

        public async Task<Page<InventoryDataResource>> SaveInventoryDataForRack(int inventoryId, string rackId, IEnumerable<InventoryDataResource> resources, CancellationToken ct)
        {
            foreach (var item in resources)
            {
                var itemEntity = _context.HomeWebInventoryData.Where(i => i.inventoryid == inventoryId && i.rackid == rackId && item.id == i.id).FirstOrDefault();
                itemEntity.state = item.state;
            }
            await _context.SaveChangesAsync(ct);
            return await GetInventoryDataForRack(inventoryId, rackId, ct);
        }

        private async Task DeleteInventoryData(int inventoryId, CancellationToken ct)
        {
            var items = await GetInventoryData(inventoryId, ct);

            foreach (var item in items.Items)
            {
                var entityToDelete = _context.HomeWebInventoryData.First(e => e.id == item.id);
                _context.HomeWebInventoryData.Remove(entityToDelete);
            }
            await _context.SaveChangesAsync(ct);
        }

        private Page<InventoryDataResource> MapInventoryDataToPage(IEnumerable<homewebbridge_inventorydata> inventoryData)
        {
            var mappedItems = _mapper.Map<IEnumerable<InventoryDataResource>>(inventoryData, opt => opt.Items[Controllers.Infrastructure.AutoMapperConstants.INLINE_COVER_IMAGE] = true);

            var page = new Page<InventoryDataResource>
            {
                Items = mappedItems,
                TotalSize = inventoryData.Count()
            };
            return page;
        }

        public async Task<InventoryResource> AbandonInventory(int id, DateTime abandonDate, CancellationToken ct)
        {
            var inventoryEntity = GetInventoryEntityById(id);
            inventoryEntity.endtime = abandonDate;
            inventoryEntity.state = (int)InventoryState.Abandoned;
            await _context.SaveChangesAsync(ct);
            return MapFromInventoryEntity(inventoryEntity);
        }

        public async Task<IEnumerable<InventoryRackModel>> GetInventoryRacks(int inventoryId, CancellationToken ct)
        {
            var resultList = new List<InventoryRackModel>();

            var availableRackIds = await _movieDataService.GetRacks(ct);

            foreach (var rackId in availableRackIds)
            {
                // prepare the rack model
                var rackModel = new InventoryRackModel { RackId = rackId };

                // check if there is already data for this rackId in inventory
                var inventoryDataForRackId = _context.HomeWebInventoryData.Where(d => d.rackid == rackId && d.inventoryid == inventoryId);
                var hasNoInventoryData = inventoryDataForRackId.Count() == 0;
                if (hasNoInventoryData)
                {
                    _logger.LogDebug($"No data for rack with id {rackId} found in inventory with id {inventoryId}");
                    rackModel.RackInventoryState = InventoryState.NotStarted;
                    rackModel.RackState = InventoryDataState.NotChecked;
                }
                else
                {
                    //check if there are movies in this rackid which are not yet checked. If so, rackid is not completed!
                    var hasNotChecked = inventoryDataForRackId.Where(i => i.state == (int)InventoryDataState.NotChecked).Count() > 0;
                    if(hasNotChecked)
                    {
                        _logger.LogDebug($"Data for rack with id {rackId} found in inventory with id {inventoryId} in state not checked");
                        rackModel.RackInventoryState = InventoryState.Started;
                        rackModel.RackState = InventoryDataState.NotChecked;
                    }
                    else
                    {
                        rackModel.RackInventoryState = InventoryState.Finished;
                        var hasMissing = inventoryDataForRackId.Where(i => i.state == (int)InventoryDataState.Missing).Count() > 0;
                        if (hasMissing)
                        {
                            _logger.LogDebug($"Found complete data for rack with id {rackId} in inventory with id {inventoryId}, but there are some missing entries.");
                            rackModel.RackState = InventoryDataState.Missing;
                        }
                        else 
                        {
                            _logger.LogDebug($"Found complete data for rack with id {rackId} in inventory with id {inventoryId}, all entries are marked as found.");
                            rackModel.RackState = InventoryDataState.Found;
                        }
                    }
                }

                // finally, add rack model to the result list
                resultList.Add(rackModel);
            }
            return resultList;
        }
    }
}

