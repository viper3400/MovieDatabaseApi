using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared;
using Jaxx.WebApi.Shared.Controllers.Infrastructure;
using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Controllers
{
    [Route("/[controller]")]
    [OpenApiTag("Inventory", Description = "ApiController to handle inventory operations.")]
    [Authorize(Policy = "VideoDbUser")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly PagingOptions _defaultPagingOptions;
        private readonly ILogger _logger;
        private readonly IUserContextInformationProvider userContextProvider;

        public InventoryController(
         IInventoryService inventoryService,
         IOptions<PagingOptions> defaultPagingOptionsAccessor,
         ILogger<InventoryController> logger,
         IUserContextInformationProvider userContextProvider)
        {
            _inventoryService = inventoryService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
            _logger = logger;
            this.userContextProvider = userContextProvider;
        }

        [HttpPost(Name = nameof(CreateInventory))]
        [OpenApiOperation("Create a new inventory", "Provide a name for the new inventory.")]
        [SwaggerResponse(200, typeof(InventoryResource), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> CreateInventory(
            [FromQuery] string name,
            CancellationToken ct)
        {
            var createdInventory = await _inventoryService.CreateInventory(name, DateTime.Now, ct);
            return Ok(createdInventory);
        }


        [HttpGet(Name = nameof(GetInventories))]
        [OpenApiOperation("Get all inventories", "Returns all recorded inventory.")]
        [SwaggerResponse(200, typeof(CollectionWithPaging<InventoryResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]

        public async Task<IActionResult> GetInventories(
            [FromQuery] string nameFilter,
            [FromQuery] PagingOptions pagingOptions,
            CancellationToken ct)
        {
            /*pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;*/

            pagingOptions.Offset ??= _defaultPagingOptions.Offset;
            pagingOptions.Limit ??= _defaultPagingOptions.Limit;

            var items = await _inventoryService.GetInventories(nameFilter, pagingOptions, ct);

            var collection = CollectionWithPaging<InventoryResource>.Create(
                Link.ToCollection(nameof(GetInventories)),
                items.Items.ToArray(),
                items.TotalSize,
                pagingOptions);

            return Ok(collection);
        }

        [HttpGet("{Id}", Name = nameof(GetInventory))]
        [OpenApiOperation("Get inventory by id", "Return an inventory with the given id.")]
        [SwaggerResponse(200, typeof(InventoryResource), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public IActionResult GetInventory(
            GetByGenericIdParameter parameters)
        {
            var item = _inventoryService.GetInventory(parameters.Id);

            return Ok(item);
        }

        [HttpDelete("{Id}", Name = nameof(DeleteInventory))]
        [OpenApiOperation("Delete inventory", "Delete the inventory with the given id.")]
        [SwaggerResponse(200, typeof(bool), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> DeleteInventory(
         GetByGenericIdParameter parameters,
         CancellationToken ct)
        {
            var result = await _inventoryService.DeleteInventory(parameters.Id, ct);
            return Ok(result);

        }

        [HttpGet("{Id}/rack/{RackId}", Name = nameof(GetInventoryRackDataForRack))]
        [OpenApiOperation("Get rack for inventory by id and rack id", "Returns a rack.")]
        [SwaggerResponse(200, typeof(CollectionWithPaging<InventoryDataResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> GetInventoryRackDataForRack(
            GetByGenericIdParameter parameters,
            GetRackIdFromRouteParameter rackIdParameter,
            PagingOptions pagingOptions,
            CancellationToken ct
        )
        {
            var data = await _inventoryService.GetInventoryDataForRack(parameters.Id, rackIdParameter.RackId, ct);
            var collection = CollectionWithPaging<InventoryDataResource>.Create(
                Link.ToCollection(nameof(GetInventoryRackDataForRack)), data.Items.ToArray(), data.TotalSize, pagingOptions);
            return Ok(collection);
        }

        [HttpGet("{Id}/rack", Name = nameof(GetInventoryRacks))]
        [OpenApiOperation("Get all racks and bays from moviedata for inventory by id and state", "Returns racks and states.")]
        [SwaggerResponse(200, typeof(IEnumerable<InventoryRackModel>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> GetInventoryRacks(
        GetByGenericIdParameter parameters,
        CancellationToken ct
        )
        {
            var data = await _inventoryService.GetInventoryRacks(parameters.Id, ct);
            return Ok(data);
        }

        [HttpPost("{Id}/rack/{RackId}", Name = nameof(PostInventoryRack))]
        [OpenApiOperation("Post rack for inventory by id and rack id", "")]
        [SwaggerResponse(200, typeof(CollectionWithPaging<InventoryDataResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> PostInventoryRack(
            GetByGenericIdParameter parameters,
            GetRackIdFromRouteParameter rackIdParameter,
            PagingOptions pagingOptions,
            [FromBody] IEnumerable<InventoryDataResource> inventoryDataResources,
            CancellationToken ct
        )
        {
            var result = await _inventoryService.SaveInventoryDataForRack(parameters.Id, rackIdParameter.RackId, inventoryDataResources, ct);
            var collection = CollectionWithPaging<InventoryDataResource>.Create(
                Link.ToCollection(nameof(GetInventoryRackDataForRack)), result.Items.ToArray(), result.TotalSize, pagingOptions);
            return Ok(collection);
        }

        [HttpPost("{Id}/finish", Name = nameof(FinishInventory))]
        [OpenApiOperation("Finish inventory", "Finishs the inventory with the given id.")]
        [SwaggerResponse(200, typeof(InventoryResource), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> FinishInventory(
            GetByGenericIdParameter parameters,
            CancellationToken ct)
        {
            var finishedInventory = await _inventoryService.FinishInventory(parameters.Id, DateTime.Now, ct);
            return Ok(finishedInventory);
        }

        [HttpPost("{Id}/abandon", Name = nameof(AbandonInventory))]
        [OpenApiOperation("Abandon inventory", "Abandon the inventory with the given id.")]
        [SwaggerResponse(200, typeof(InventoryResource), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> AbandonInventory(
    GetByGenericIdParameter parameters,
    CancellationToken ct)
        {
            var finishedInventory = await _inventoryService.AbandonInventory(parameters.Id, DateTime.Now, ct);
            return Ok(finishedInventory);
        }
    }
}
