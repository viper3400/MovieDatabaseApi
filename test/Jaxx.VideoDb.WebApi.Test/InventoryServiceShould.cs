using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Xunit;
using Jaxx.WebApi.Shared.Models;
using System.Threading;

namespace Jaxx.VideoDb.WebApi.Test
{
    [Trait("Feature", "Inventory")]
    public class InventoryServiceShould
    {
        private readonly IInventoryService _movieInventoryService;
        private InventoryService _movieInventoryServiceByPassInterface;
        private readonly string _userName;
        private readonly string _viewGroup;
        private readonly MovieDataServiceOptions _movieDataServiceOptions;

        public InventoryServiceShould()
        {

            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            _movieInventoryService = host.Services.GetService(typeof(IInventoryService)) as IInventoryService;

            _userName = TestMovieDataServiceHost.UserName;
            _viewGroup = TestMovieDataServiceHost.ViewGroup;
            _movieDataServiceOptions = TestMovieDataServiceHost.MovieDataServiceOptions;
            _movieInventoryServiceByPassInterface = _movieInventoryService as InventoryService;
        }

        [Fact]
        public async void CreateReadDeleteInventory()
        {
            // Tear up test
            var startDate = DateTime.Now;
            var inventoryName = $"UTest-{nameof(CreateReadDeleteInventory)}-{startDate.Ticks}";

            // Test
            var actual = await _movieInventoryService.CreateInventory(inventoryName, startDate, new CancellationToken());
            Assert.IsType<InventoryResource>(actual);
            Assert.IsType<int>(actual.id);
            Assert.Equal(inventoryName, actual.name);
            Assert.Equal(startDate, actual.starttime);
            Assert.Null(actual.endtime);
            Assert.Equal((int)InventoryState.Started, actual.state);

            var readInventory = _movieInventoryService.GetInventory(actual.id);
            Assert.Equal(actual.id, readInventory.id);
            Assert.Equal(inventoryName, readInventory.name);
            Assert.Equal(startDate, actual.starttime);
            Assert.Null(readInventory.endtime);
            Assert.Equal((int)InventoryState.Started, readInventory.state);

            var deleteInventory = await _movieInventoryService.DeleteInventory(readInventory.id, new CancellationToken());
            Assert.True(deleteInventory);

            var checkInventory = _movieInventoryService.GetInventory(readInventory.id);
            Assert.Null(checkInventory);
        }

        [Fact]
        public async void StartFinishInventory()
        {
            // Tear up test
            var startDate = DateTime.Now;
            var endDate = startDate + new TimeSpan(1, 0, 0);
            var inventoryName = $"UTest-{nameof(StartFinishInventory)}-{startDate.Ticks}";

            // Test
            var sut = await _movieInventoryService.CreateInventory(inventoryName, startDate, new CancellationToken());

            var started = _movieInventoryService.GetInventory(sut.id);
            Assert.Equal(startDate, started.starttime);
            Assert.Equal((int)InventoryState.Started, started.state);

            await _movieInventoryService.FinishInventory(sut.id, endDate, new CancellationToken());

            var finished = _movieInventoryService.GetInventory(sut.id);
            Assert.Equal(startDate, finished.starttime);
            Assert.Equal(endDate, finished.endtime);
            Assert.Equal((int)InventoryState.Finished, finished.state);

            // Tear down test
            await _movieInventoryService.DeleteInventory(sut.id, new CancellationToken());
        }

        [Fact]
        public async void NotFinishAFinishedInventory()
        {
            var startTime = DateTime.Now;
            var finishTime = startTime + new TimeSpan(1, 0, 0);
            var finishTimeNext = startTime + new TimeSpan(2, 0, 0);

            var sut = await _movieInventoryService.CreateInventory(nameof(NotFinishAFinishedInventory), startTime, new CancellationToken());
            await _movieInventoryService.FinishInventory(sut.id, finishTime, new CancellationToken());
            Task act() => _movieInventoryService.FinishInventory(sut.id, finishTimeNext, new CancellationToken());
            var exception = await Assert.ThrowsAsync<NotSupportedException>(act);
            Assert.Equal($"Inventory with {sut.id} has alreay been finished and could not be finished twice.", exception.Message);

            await _movieInventoryService.DeleteInventory(sut.id, new CancellationToken());
        }

        [Fact]
        public async void GetAllInventories()
        {
            var inventories = new List<InventoryResource>();
            for (var i = 1; i <= 20; i++)
            {
                var inventory = await _movieInventoryService.CreateInventory($"{nameof(GetAllInventories)} - Test Inventory No {i}", DateTime.Now, new CancellationToken());
                inventories.Add(inventory);
            }

            var actual = await _movieInventoryService.GetInventories($"{nameof(GetAllInventories)} - Test Inventory No", new PagingOptions { Limit = 50, Offset = 0 }, new CancellationToken());
            Assert.Equal(20, actual.TotalSize);

            var all = await _movieInventoryService.GetInventories(null, new PagingOptions { Limit = 500, Offset = 0 }, new CancellationToken());
            Assert.True(actual.TotalSize >= 20);
            foreach (var result in actual.Items)
            {
                await _movieInventoryService.DeleteInventory(result.id, new CancellationToken());
            }

            /*var tasks = new List<Task<InventoryResource>>();
            for (var i = 1; i <= 20; i++)
            {
                tasks.Add(_movieInventoryService.CreateInventory($"Test Inventory No {i}", new System.Threading.CancellationToken()));
            }
            await Task.WhenAll(tasks);

            var actual = await _movieInventoryService.GetInventories(new Jaxx.WebApi.Shared.Models.PagingOptions(), new System.Threading.CancellationToken());
            Assert.Equal(20, actual.TotalSize);

            var deleteTasks = new List<Task<bool>>();
            foreach(var result in actual.Items)
            {
                deleteTasks.Add(_movieInventoryService.DeleteInventory(result.id, new System.Threading.CancellationToken()));
            }
            await Task.WhenAll(deleteTasks);*/
        }

        [Fact]
        public async void GetRackData()
        {
            // Tear up test
            var startDate = DateTime.Now;
            var rackId = "R05F2";
            var inventoryName = $"UTEST-{nameof(GetRackData)}-{rackId}-{startDate.Ticks}";
            var inventory = await _movieInventoryService.CreateInventory(inventoryName, startDate, new CancellationToken());

            // Test
            var result = await _movieInventoryService.GetInventoryDataForRack(inventory.id, rackId, new CancellationToken());
            Assert.Equal(17, result.TotalSize);

            var resultExisting = await _movieInventoryService.GetInventoryDataForRack(inventory.id, rackId, new CancellationToken());
            Assert.Equal(17, resultExisting.TotalSize);

            // Tear down test
            await _movieInventoryService.DeleteInventory(inventory.id, new CancellationToken());
            var deletedInvetory = _movieInventoryService.GetInventory(inventory.id);
            Assert.True(deletedInvetory == null, "Error in tear down test");
        }

        [Fact]
        public async void AbandonInventory()
        {
            // Tear up test
            var startDate = DateTime.Now;
            var abandonDate = DateTime.Now.AddDays(1);
            var rackId = "R05F2";
            var inventoryName = $"UTEST-{nameof(GetRackData)}-{rackId}-{startDate.Ticks}";
            var inventory = await _movieInventoryService.CreateInventory(inventoryName, startDate, new CancellationToken());

            // Test
            InventoryResource result = await _movieInventoryService.AbandonInventory(inventory.id, abandonDate, new CancellationToken());
            Assert.Equal((int)InventoryState.Abandoned, result.state);

            // Tear down test
            await _movieInventoryService.DeleteInventory(inventory.id, new CancellationToken());
            var deletedInvetory = _movieInventoryService.GetInventory(inventory.id);
            Assert.True(deletedInvetory == null, "Error in tear down test");
        }
    }
}
