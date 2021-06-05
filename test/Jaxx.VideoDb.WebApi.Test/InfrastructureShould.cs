using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Configuration;
using Jaxx.VideoDb.WebCore.Controllers.Infrastructure;
using System.Linq;
using System.Security.Claims;
using Jaxx.WebApi.Shared.Controllers.Infrastructure;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class InfrastructureShould
    {
        [Fact]
        public void ExtractClaimFromToken()
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("ClientSecrets.json")
                .AddJsonFile("testsettings.json")
                .AddJsonFile("appsettings.json")
                .Build();

            var key = Configuration["Jwt:Key"];
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Imphbi5ncmFlZmUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2dyb3Vwc2lkIjoiVkdfRGVmYXVsdCIsImV4cCI6MTU3Nzk3MDkwNywiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDY0NyIsImF1ZCI6WyJodHRwOi8vbG9jYWxob3N0OjUwNjQ3IiwiaHR0cDovL2xvY2FsaG9zdDo1MDY0NyJdfQ.WfsLGKTGTkvYd81HpxpyZDEiNy0WEfE_-N376ztNb1g";
            var principal = ClaimsHelper.GetPrincipalFromToken(token, key);
            var username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var group = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid).Value;
            Assert.Equal("jan.graefe", username);
            Assert.Equal("VG_Default", group);

            var usermodel = ClaimsHelper.GetUserModelFromToken(token, key);
            Assert.Equal("jan.graefe", usermodel.Name);
            Assert.Equal("VG_Default", usermodel.Groups.FirstOrDefault());

        }
    }
}
