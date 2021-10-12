using AutoMapper;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test.API
{
    [Collection("AutoMapperCollection")]
    public class MovieDataRouteShould
    {
        private readonly HttpClient _client;
        private readonly string _userName;
        private readonly string _viewGroup;
        private readonly string _apiMasterKey;

        private WebCore.Services.IMovieDataService _movieDataService;

        public MovieDataRouteShould(AutoMapperFixture fixture)
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

            _movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;

        }

        private void AddDefaultHeaders ()
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
        public async Task GetOKResponse()
        {
            var response = await _client.GetAsync("/MovieData");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetMovieDataWithFilterForDiskId ()
        {
            // In this test we like to get all movies from location R15 using the diskid filter
            var movieDataOptions = new MovieDataOptions { Diskid = "R15" };
            var movieDataOptionsJson = JsonConvert.SerializeObject(movieDataOptions);

            var expectedCount = 107;

            var response = await _client.PostAsync("/MovieData", new StringContent(movieDataOptionsJson, Encoding.UTF8, "application/json"));
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;

            Assert.Equal(expectedCount, actualCount);

        }

        [Fact]
        public async Task GetMovieDataByTitle()
        {            
            var movieDataOptions = new MovieDataOptions { Title = "Taffe Mädels" };
            var movieDataOptionsJson = JsonConvert.SerializeObject(movieDataOptions);

            var expectedCount = 1;

            var response = await _client.PostAsync("/MovieData", new StringContent(movieDataOptionsJson, Encoding.UTF8, "application/json"));
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(expectedCount, actualCount);

            var id = (int)collection.value[0].id;
            Assert.Equal(1865, id);
        }

        [Fact]
        public async Task GetMovieDataByTitleCaseInsesitive()
        {
            var movieDataOptions = new MovieDataOptions { Title = "taffe Mädels" };
            var movieDataOptionsJson = JsonConvert.SerializeObject(movieDataOptions);

            var expectedCount = 1;

            var response = await _client.PostAsync("/MovieData", new StringContent(movieDataOptionsJson, Encoding.UTF8, "application/json"));
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(expectedCount, actualCount);

            var id = (int)collection.value[0].id;
            Assert.Equal(1865, id);

        }

        [Fact]
        public async Task GetMovieDataWithFilterForMediaType()
        {            
            var expectedCount = 164;
            var response = await _client.GetAsync("/MovieData/?mediatypes=15");
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async Task GetMovieDataWithFilterForListOfMediaTypes()
        {
            var expectedCount = 1648;

            var response = await _client.GetAsync("/MovieData/?mediatypes=15,16");
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public async Task GetMovieDataWithCombinedFilterForDiskIdAndTitle()
        {
            // In this test we like to get all movies from location R15 using the diskid filter
            var movieDataOptions = new MovieDataOptions { Diskid = "R15" };
            var movieDataOptionsJson = JsonConvert.SerializeObject(movieDataOptions);

            var expectedCount = 107;

            var response = await _client.PostAsync("/MovieData", new StringContent(movieDataOptionsJson, Encoding.UTF8, "application/json"));
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;

            Assert.Equal(expectedCount, actualCount);

        }

        [Fact]
        public async Task ProvidePagingHrefs()
        {
            var response = await _client.GetAsync("/MovieData?search=R20");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var expectedNextLink = "https://danielgraefe.de/api/videodb/beta/moviedata?Search=R20&SortOrder=ByDiskIdAsc&UseInlineCoverImage=False&ExactMatch=False&limit=25&offset=25";
            var actualNextLink = (string)collection.next.href;
            Assert.Equal(expectedNextLink, actualNextLink);
        }


        [Fact]
        public async Task GetMovieSeenData()
        {
            var expectedCount = 1908;

            var response = await _client.GetAsync("/MovieData/seen");
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var actualCount = (int)collection.size;

            Assert.Equal(expectedCount, actualCount);

        }

        [Fact]
        public async Task SetMovieSeenUnseen()
        {
            var movieSeenOptions = new MovieSeenOptions
            {
                Id = 2135,
                Date = "2018-08-18"
            };

            var json = JsonConvert.SerializeObject(movieSeenOptions);
            var response = await _client.PostAsync("/MovieData/Seen", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var expectedResultMessage = $"Set movie with id 2135 seen for jan.graefe in viewgroup VG_Default for date 18.08.2018.";
            Assert.Equal(expectedResultMessage, (string)collection.value[0].item2);

            // it should not be possible  to add a movie seen twice at the same date
            response = await _client.PostAsync("/MovieData/Seen", new StringContent(json, Encoding.UTF8, "application/json"));
            collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            expectedResultMessage = $"Entry for movie with id 2135 for jan.graefe in viewgroup VG_Default for date 18.08.2018 allready exists.";
            Assert.Equal(expectedResultMessage, (string)collection.value[0].item2);

            // delete the movie from seen list (we reuse the object from before)            
            json = JsonConvert.SerializeObject(movieSeenOptions);
            var request = new HttpRequestMessage(HttpMethod.Delete, "/MovieData/Seen");            
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            response = await _client.SendAsync(request);
            // response = await _client.DeleteAsync("/MovieData/setSeen", new StringContent(json, Encoding.UTF8, "application/json"));
            collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            expectedResultMessage = $"Removed movie seen date for id 2135 in viewgroup VG_Default for date 18.08.2018.";
            Assert.Equal(expectedResultMessage, (string)collection.value[0].item2);

            // we cant delete a movie which not exists
            var secondDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/MovieData/Seen");
            secondDeleteRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            response = await _client.SendAsync(secondDeleteRequest);
            collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            expectedResultMessage = $"No seen entry found for movie with id 2135 in viewgroup VG_Default for date 18.08.2018.";
            Assert.Equal(expectedResultMessage, (string)collection.value[0].item2);

        }

        [Fact]
        public async void GetFavorites()
        {
            var response = await _client.GetAsync("/MovieData/favorites");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(145, (int)collection.size);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void GetWatchAgainMovies()
        {
            var response = await _client.GetAsync("/MovieData/watchagain");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(108, (int)collection.size);
        }

        [Fact]
        public async void UpdateMovie()
        {
            var getResponse = await _client.GetAsync("/MovieData/5");
            dynamic movie = JObject.Parse(getResponse.Content.ReadAsStringAsync().Result);
            Assert.Equal("Big Fish", movie.title.ToString());

            movie.title = "Big Fish (updated by API Test)";
            var updateJson = JsonConvert.SerializeObject(movie);
            

            var response = await _client.PostAsync("/MovieData/5", new StringContent(updateJson, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Big Fish (updated by API Test)", actual.title.ToString());

            movie.title = "Big Fish";
            updateJson = JsonConvert.SerializeObject(movie);

            response = await _client.PostAsync("/MovieData/5", new StringContent(updateJson, Encoding.UTF8, "application/json"));
            actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal("Big Fish", actual.title.ToString());
        }

        [Fact]
        public async void CreateMovie()
        {
            var newMovie = new MovieDataResource
            {
                title = "MyNewMovie",
                diskid = "R35F8D14",
                owner_id = 3,
                mediatype = 16
            };

            var json = JsonConvert.SerializeObject(newMovie);
            var response = await _client.PostAsync("/MovieData/0", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("MyNewMovie", actual.title.ToString());

            int id = int.Parse(actual.id.ToString());

            // we will now use an IMovieDataService to clean up our test an delete 
            // the created entity because the "hard" delete function won't exposed to 
            // API for now
            var createdMovie = await _movieDataService.GetMovieDataAsync(id, new System.Threading.CancellationToken());
            Assert.Equal("MyNewMovie", createdMovie.title);
            await _movieDataService.DeleteMovieDataAsync(id, new System.Threading.CancellationToken());
        }

        [Fact]
        public async void CreateMovieWithInvalidDiskId()
        {
            var newMovie = new MovieDataResource
            {
                title = "MyNewMovie",                
                owner_id = 3,
                mediatype = 16
            };

            var json = JsonConvert.SerializeObject(newMovie);
            var response = await _client.PostAsync("/MovieData/0", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("<> is no valid disk id.", actual.detail.ToString());         
        }

        [Fact]
        public async void CreateMovieForNonDiskIdValidateMediaType()
        {
            var newMovie = new MovieDataResource
            {
                title = "MyNewMovie",
                diskid = "",
                owner_id = 3,
                mediatype = 2
            };

            var json = JsonConvert.SerializeObject(newMovie);
            var response = await _client.PostAsync("/MovieData/0", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("MyNewMovie", actual.title.ToString());

            int id = int.Parse(actual.id.ToString());

            // we will now use an IMovieDataService to clean up our test an delete 
            // the created entity because the "hard" delete function won't exposed to 
            // API for now
            var createdMovie = await _movieDataService.GetMovieDataAsync(id, new System.Threading.CancellationToken());
            Assert.Equal("MyNewMovie", createdMovie.title);
            await _movieDataService.DeleteMovieDataAsync(id, new System.Threading.CancellationToken());
        }

        [Fact]
        public async void CreateMovieWithGenres()
        {
            var newMovie = new MovieDataResource
            {
                title = "MyNewMovie",
                diskid = "R35F8D14",
                owner_id = 3,
                mediatype = 16,
                Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4 } }
            };

            var json = JsonConvert.SerializeObject(newMovie);
            var response = await _client.PostAsync("/MovieData/0", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("MyNewMovie", actual.title.ToString());
            Assert.Equal("Comedy", actual.genres[0].name.ToString());

            int id = int.Parse(actual.id.ToString());

            // we will now use an IMovieDataService to clean up our test an delete 
            // the created entity because the "hard" delete function won't exposed to 
            // API for now
            var createdMovie = await _movieDataService.GetMovieDataAsync(id, new System.Threading.CancellationToken());
            Assert.Equal("MyNewMovie", createdMovie.title);
            await _movieDataService.DeleteMovieDataAsync(id, new System.Threading.CancellationToken());
        }

        [Fact]
        public async void HandleMovieUserSettingsWatchAgain()
        {
            var setting = new MovieUserSetting { IsFlagged = true };
            var json = JsonConvert.SerializeObject(setting);
            var response = await _client.PostAsync("/MovieData/watchagain/5", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(bool.Parse(actual.isFlagged.ToString()));

            setting.IsFlagged = false;
            json = JsonConvert.SerializeObject(setting);
            response = await _client.PostAsync("/MovieData/watchagain/5", new StringContent(json, Encoding.UTF8, "application/json"));
            actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(bool.Parse(actual.isFlagged.ToString()));
        }

        [Fact]
        public async void HandleMovieUserSettingsFavorite()
        {
            var setting = new MovieUserSetting { IsFavorite = true };
            var json = JsonConvert.SerializeObject(setting);
            var response = await _client.PostAsync("/MovieData/favorites/5", new StringContent(json, Encoding.UTF8, "application/json"));
            dynamic actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(bool.Parse(actual.isFavorite.ToString()));

            setting.IsFavorite = false;
            json = JsonConvert.SerializeObject(setting);
            response = await _client.PostAsync("/MovieData/favorites/5", new StringContent(json, Encoding.UTF8, "application/json"));
            actual = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(bool.Parse(actual.isFavorite.ToString()));
        }

        [Fact]
        public async void ReturnAllMediaTypes()
        {

            var response = await _client.GetAsync("/MovieData/mediatypes");
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            //Assert.Equal(18, (int)collection.size);
            Assert.Equal(7, (int)collection.size);
            //Assert.Equal("Blu-ray", collection.value[15].Name);
            // Assert.Equal("HDD", actual.FirstOrDefault(i => i.Id == 14).Name);
        }

        [Fact]
        public async void ReturnAllGenres()
        {

            var response = await _client.GetAsync("/MovieData/genres");
            dynamic collection = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(26, (int)collection.size);
            //Assert.Equal("Blu-ray", collection.value[15].Name);
            // Assert.Equal("HDD", actual.FirstOrDefault(i => i.Id == 14).Name);
        }

    }
}
