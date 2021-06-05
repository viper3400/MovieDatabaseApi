using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test.API
{
    public class MovieMetaRouteShould
    {
        private readonly HttpClient _client;
        private readonly string _userName;
        private readonly string _userPassword;
        private readonly string _viewGroup;
        private readonly string _apiMasterKey;

        public MovieMetaRouteShould()
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

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async Task GetMovieCollectionByTitleTheMovieDdb ()
        {
            var response = await _client.GetAsync("/MovieMeta/searchtitle/Batman?engine=TheMovieDb");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(20, actualCount);
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async Task GetMovieCollectionByTitleOfdb()
        {
            var response = await _client.GetAsync("/MovieMeta/searchtitle/Batman?engine=Ofdb");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(94, actualCount);
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async Task GetMovieCollectionByTitleOfdbDefault()
        {
            var response = await _client.GetAsync("/MovieMeta/searchtitle/Batman");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(94, actualCount);
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async Task GetMovieCollectionByReference()
        {
            var response = await _client.GetAsync("/MovieMeta/277170");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(1, actualCount);
            Assert.Equal("Kirschblüten und rote Bohnen", (string)collection.value[0].title);
        }

        [Fact]
        [Trait("Category", "Offline")]
        public async Task GetConvertedMovieDataResourceFromMovieMetaResource()
        {
            var jsonString = File.ReadAllText("./TestAssets/SingleMovieMetaResourceResponse.json");
            
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var convertedResponse = await _client.PostAsync("/MovieMeta/convert", stringContent);
            Assert.Equal(HttpStatusCode.OK, convertedResponse.StatusCode);

            dynamic converted = JObject.Parse(convertedResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal("Am Ende eines viel zu kurzen Tages", (string)converted.title);
            Assert.Equal("160", (string)converted.runtime);
            Assert.Contains("Aisling Loftus::::ofdb::", (string)converted.actors);
            Assert.Equal(7, (int)converted.genres[0].id);
            // Assert.Equal("Drama", (string)converted.genres[0].name);
        }

        [Fact]
        [Trait("Category", "Offline")]
        public async Task GetConvertedMovieDataResourceFromIncompleteMovieMetaResource()
        {
            var jsonString = File.ReadAllText("./TestAssets/SingleMovieMetaResourceResponseIncomplete.json");

            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var convertedResponse = await _client.PostAsync("/MovieMeta/convert", stringContent);
            Assert.Equal(HttpStatusCode.OK, convertedResponse.StatusCode);

            dynamic converted = JObject.Parse(convertedResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal("Am Ende eines viel zu kurzen Tages", (string)converted.title);
            Assert.NotNull(converted.runtime);
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async Task GetMovieCollectionByBarcode()
        {
            var response = await _client.GetAsync("/MovieMeta/searchbarcode/4009750396872");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(1, actualCount);
            Assert.Equal("Am Ende eines viel zu kurzen Tages", (string)collection.value[0].title);
        }
    }
}