using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
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
    public class TokenRouteShould
    {
        private readonly HttpClient _client;
        private readonly string _userName;
        private readonly string _userPassword;
        private readonly string _viewGroup;
        private readonly string _apiMasterKey;

        public TokenRouteShould()
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
            _userPassword = TestMovieDataServiceHost.UserPassword;
        }

        [Fact]
        public async Task ProvideAnAuthTokenWithApiMasterkeyAuth()
        {
            var jsonString = $"{{ \"username\" : \"{_userName}\", \"apimasterkey\" : \"{_apiMasterKey}\", \"password\" : \"\", \"group\" : \"{_viewGroup}\" }}";
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/token", stringContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            dynamic token = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty((string)token.SelectToken("token"));
            Assert.True(string.IsNullOrWhiteSpace((string)token.SelectToken("refreshToken")));
        }


        [Fact]
        public async Task ProvideAnAuthTokenWithPasswordAuth()
        {
            var jsonString = $"{{ \"username\" : \"{_userName}\", \"password\" : \"{_userPassword.Replace("\"","\\\"")}\", \"group\" : \"{_viewGroup}\" }}";
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/token", stringContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            dynamic token = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty((string)token.SelectToken("token"));
            Assert.True(string.IsNullOrWhiteSpace((string)token.SelectToken("refreshToken")));
        }

    }
}
