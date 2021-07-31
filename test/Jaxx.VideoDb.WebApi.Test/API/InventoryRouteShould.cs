using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test.API
{
    public class InventoryRouteShould
    {

        private readonly HttpClient _client;
        private readonly string _userName;
        private readonly string _viewGroup;
        private readonly string _apiMasterKey;

        public InventoryRouteShould()
        {
            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            var configuration = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            // Arrange
            var server = new TestServer(new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseStartup<Startup>());
            _client = server.CreateClient();

            _userName = TestMovieDataServiceHost.UserName;
            _viewGroup = TestMovieDataServiceHost.ViewGroup;
            _apiMasterKey = TestMovieDataServiceHost.ApiMasterKey;
            AddDefaultHeaders();
        }

        private void AddDefaultHeaders()
        {
            if (_client == null) throw new NotSupportedException("Client is not initialized.");

            var jsonString = $"{{ \"username\" : \"{_userName}\", \"apimasterkey\" : \"{_apiMasterKey}\", \"password\" : \"\", \"group\" : \"{_viewGroup}\" }}";
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = _client.PostAsync("/token", stringContent);
            dynamic token = JObject.Parse(response.Result.Content.ReadAsStringAsync().Result);

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.token}");
            string _ContentType = "application/json";
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));
        }

        private Task<HttpResponseMessage> PostCreateInventory(string inventoryName)
        {
            return _client.PostAsync($"/inventory?name={inventoryName}", new StringContent("", Encoding.UTF8, "application/json"));
        }

        private Task<HttpResponseMessage> GetInventory(int inventoryId)
        {
            return _client.GetAsync($"/inventory/{inventoryId}");
        }

        private Task<HttpResponseMessage> GetInventories(string nameFilter)
        {
            return _client.GetAsync($"/inventory?nameFilter={nameFilter}");
        }

        private Task<HttpResponseMessage> DeleteInventory(int inventoryId)
        {
            return _client.DeleteAsync($"/inventory/{inventoryId}");
        }
        private Task<HttpResponseMessage> GetRack(int inventoryId, string rackId)
        {
            return _client.GetAsync($"/inventory/{inventoryId}/rack/{rackId}");
        }

        private Task<HttpResponseMessage> ClientGetRacksAndState(int inventoryId)
        {
            return _client.GetAsync($"/inventory/{inventoryId}/rack");
        }

        private Task<HttpResponseMessage> PostRack(int inventoryId, string rackId, string json)
        {
            return _client.PostAsync($"/inventory/{inventoryId}/rack/{rackId}", new StringContent(json, Encoding.UTF8, "application/json"));
        }

        private Task<HttpResponseMessage> PostFinish(int inventoryId)
        {
            return _client.PostAsync($"/inventory/{inventoryId}/finish", new StringContent("", Encoding.UTF8, "application/json"));
        }

        private Task<HttpResponseMessage> PostAbandon(int inventoryId)
        {
            return _client.PostAsync($"/inventory/{inventoryId}/abandon", new StringContent("", Encoding.UTF8, "application/json"));
        }

        /// <summary>
        /// Create, retrieve and delete an inventory
        /// </summary>
        [Fact]
        public async void CreateGetAndDeleteNewInventory()
        {
            var inventoryName = $"ITest-{nameof(CreateGetAndDeleteNewInventory)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            dynamic jsonResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());
            Assert.Equal(inventoryName, (string)jsonResult.name);
            Assert.IsType<int>((int)jsonResult.id);
            Assert.Equal(InventoryState.Started, Enum.Parse<InventoryState>((string)jsonResult.state));
            // TODO: https://gitlab.com/viper3400/jaxx.net.videodb.api/-/issues/45 
            Assert.True((DateTime)jsonResult.starttime > DateTime.Now.AddHours(-3) && (DateTime)jsonResult.starttime < DateTime.Now.AddHours(+3), $"starttime inconclusive: {jsonResult.starttime}");
            Assert.True(string.IsNullOrWhiteSpace((string)jsonResult.endtime));

            var inventoryId = (int)jsonResult.id;

            var getResponse = await GetInventory(inventoryId);
            dynamic jsonGetResult = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(inventoryName, (string)jsonGetResult.name);
            Assert.Equal(InventoryState.Started, Enum.Parse<InventoryState>((string)jsonGetResult.state));
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }

        [Fact]
        public async void GetAllInventories()
        {
            // Tear up test 
            var inventories = new List<int>();
            var timestamp = DateTime.Now.Ticks;
            for (var i = 1; i <= 20; i++)
            {
                var inventory = await PostCreateInventory($"ITEST-{nameof(GetAllInventories)}-{timestamp}-Test Inventory No {i}");
                dynamic jsonResult = JObject.Parse(await inventory.Content.ReadAsStringAsync());
                inventories.Add((int)jsonResult.id);
            }

            // Test
            var actual = await GetInventories($"ITEST-{nameof(GetAllInventories)}-{timestamp}-Test Inventory No");
            dynamic actualJsonResult = JObject.Parse(await actual.Content.ReadAsStringAsync());
            Assert.Equal(20, (int)actualJsonResult.size);

            var all = await GetInventories(null);
            dynamic allJsonResult = JObject.Parse(await actual.Content.ReadAsStringAsync());
            Assert.True((int)allJsonResult.size >= 20);

            // Tear down test
            foreach (var id in inventories)
            {
                await DeleteInventory(id);
            }
        }


        [Fact]
        public async void GetInventoryForRack()
        {
            // Prepare a new inventory for Rack under test
            var rackId = "R05F2";
            var expectedSize = 12;
            var inventoryName = $"ITest-{nameof(GetInventoryForRack)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            dynamic creaeteResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());

            var inventoryId = (int)creaeteResult.id;

            // Test
            var getResponse = await GetRack(inventoryId, rackId);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            dynamic actual = JObject.Parse(getResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedSize, (int)actual.size);

            // Tear down test
            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }

        [Fact]
        public async void PostInventoryForRack()
        {
            // Prepare a new inventory for Rack under test
            var rackId = "R05F2";
            var expectedSize = 12;
            var inventoryName = $"ITest-{nameof(GetInventoryForRack)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            dynamic creaeteResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());

            var inventoryId = (int)creaeteResult.id;

            // Test
            var getResponse = await GetRack(inventoryId, rackId);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            dynamic actual = JObject.Parse(getResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedSize, (int)actual.size);

            var first = actual.value[0];
            var postList = new List<InventoryDataResource>();
            postList.Add(new InventoryDataResource { id = first.id, inventoryid = first.inventoryid, movieid = first.movieid, state = 2 });

            var postResult = await PostRack(inventoryId, rackId, JsonConvert.SerializeObject(postList));

            var getResponseAfterPost = await GetRack(inventoryId, rackId);
            Assert.Equal(HttpStatusCode.OK, getResponseAfterPost.StatusCode);
            dynamic actualAfterPost = JObject.Parse(getResponseAfterPost.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedSize, (int)actual.size);
            Assert.Equal((int)InventoryDataState.Missing, (int)actualAfterPost.value[0].state);

            // Tear down test
            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }

        [Fact]
        public async void FinishInventory()
        {
            // Tear up test 
            // Prepare a new inventory for Rack under test
            var inventoryName = $"ITest-{nameof(FinishInventory)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            dynamic createResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());

            var inventoryId = (int)createResult.id;

            //Test
            var result = await PostFinish(inventoryId);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            dynamic actual = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.Equal((int)InventoryState.Finished, (int)actual.state);

            // Tear down test
            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }


        [Fact]
        public async void AbandonInventory()
        {
            // Tear up test 
            // Prepare a new inventory for Rack under test
            var inventoryName = $"ITest-{nameof(AbandonInventory)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            dynamic creaeteResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());

            var inventoryId = (int)creaeteResult.id;

            //Test
            var result = await PostAbandon(inventoryId);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            dynamic actual = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.Equal((int)InventoryState.Abandoned, (int)actual.state);

            // Tear down test
            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }

        [Fact]
        public async void GetRacksAndState()
        {
            // Prepare a new inventory for Rack under test
            var rackId = "R05F2";
            var expectedSize = 12;
            var expectedRackCount = 196;
            var inventoryName = $"ITest-{nameof(GetRacksAndState)}-{DateTime.Now.Ticks}";

            var createResponse = await PostCreateInventory(inventoryName);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            dynamic creaeteResult = JObject.Parse(await createResponse.Content.ReadAsStringAsync());

            var inventoryId = (int)creaeteResult.id;

            // Test --> Expect items in RacksAndState, all of them not checked.

            var rackResponse = await ClientGetRacksAndState(inventoryId);
            IEnumerable<InventoryRackModel> racksResult = JsonConvert.DeserializeObject<IEnumerable<InventoryRackModel>>(rackResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedRackCount, racksResult.Count());
            Assert.Single(racksResult.Where(i => i.RackId == "R24F1" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResult.Where(i => i.RackId == "R05F7" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResult.Where(i => i.RackId == "R05F2" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));


            // Create a rack for the inventory and set all movies to found

            var getResponse = await GetRack(inventoryId, rackId);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            dynamic actual = JObject.Parse(getResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedSize, (int)actual.size);

            var postList = new List<InventoryDataResource>();

            foreach (var entry in actual.value)
            {
                postList.Add(new InventoryDataResource { id = entry.id, inventoryid = entry.inventoryid, movieid = entry.movieid, state = 1 });
            }

            var postResult = await PostRack(inventoryId, rackId, JsonConvert.SerializeObject(postList));

            // Test --> Expect the rack beeing completed and rack state is found

            var rackResponseAfterPost = await ClientGetRacksAndState(inventoryId);
            IEnumerable<InventoryRackModel> racksResultAfterPost = JsonConvert.DeserializeObject<IEnumerable<InventoryRackModel>>(rackResponseAfterPost.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedRackCount, racksResultAfterPost.Count());
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R24F1" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R05F7" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R05F2" && i.RackInventoryState == InventoryState.Finished && i.RackState == InventoryDataState.Found));

            // Set the first item in rack to missing

            var first = actual.value[0];
            postList = new List<InventoryDataResource>();
            postList.Add(new InventoryDataResource { id = first.id, inventoryid = first.inventoryid, movieid = first.movieid, state = 2 });
            postResult = await PostRack(inventoryId, rackId, JsonConvert.SerializeObject(postList));


            // Test --> Expect the rack beeing completed and rack state is missing
            rackResponseAfterPost = await ClientGetRacksAndState(inventoryId);
            racksResultAfterPost = JsonConvert.DeserializeObject<IEnumerable<InventoryRackModel>>(rackResponseAfterPost.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedRackCount, racksResultAfterPost.Count());
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R24F1" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R05F7" && i.RackInventoryState == InventoryState.NotStarted && i.RackState == InventoryDataState.NotChecked));
            Assert.Single(racksResultAfterPost.Where(i => i.RackId == "R05F2" && i.RackInventoryState == InventoryState.Finished  && i.RackState == InventoryDataState.Missing));

            // Tear down test
            var deleteResult = await DeleteInventory(inventoryId);
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
        }

    }
}
