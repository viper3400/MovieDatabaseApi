using Jaxx.WebApi.Shared.Identity;
using Jaxx.WebApi.Shared.Identity.MySql;
using Jaxx.WebApi.Shared.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class IdentityContextShould
    {
        private readonly ILogger<DefaultIdentityService> logger;
        public IConfiguration Configuration { get; }
        private ServiceProvider serviceProvider { get; set; }
        private IIdentityService identityService;

        private void SetupServices()
        {
            var services = new ServiceCollection();
            var connectionString = Configuration["IdentityDbConnectionString"];
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IdentityDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            services.AddTransient<IIdentityService, DefaultIdentityService>();
            services.AddLogging();

            serviceProvider = services.BuildServiceProvider();

            identityService = serviceProvider.GetService<IIdentityService>();

            var ctx = IdentityDbContextFactory.Create(connectionString);
        }
        public IdentityContextShould(ITestOutputHelper testOutputHelper)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("ClientSecrets.json")
                //.AddJsonFile("testsettings.json")
                //.AddJsonFile("appsettings.json")
                .Build();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            logger = loggerFactory.CreateLogger<DefaultIdentityService>();

            SetupServices();
        }

        [Fact(Skip = "")]
        public void CreateDatabase()
        {
            var connectionString = Configuration["IdentityDbConnectionString"];
            var ctx = IdentityDbContextFactory.Create(connectionString);
            ctx.Users.FirstOrDefault();
        }

        [Fact]
        public async void CreateReadDeleteUser()
        {
            var testUserName = "TestUser";
            var user = new IdentityUser { UserName = testUserName };
            await identityService.AddUser(user, "apitest1234", new System.Threading.CancellationToken());
            var actualUser = identityService.GetUsers().Result.Where(u => u.UserName == testUserName);

            Assert.Single(actualUser);
            Assert.Equal(testUserName, actualUser.FirstOrDefault().UserName);

            await identityService.DeleteUser(actualUser.FirstOrDefault().Id);

            var deletedUser = identityService.GetUsers().Result.Where(u => u.UserName == testUserName);
            Assert.Empty(deletedUser);
        }

        [Fact]
        public async void CreateReadDeleteRole()
        {
            var testRole = "TestRole";
            await identityService.AddRole(testRole);
            var actualRole = identityService.GetRoles(new System.Threading.CancellationToken()).Result.Where(u => u.Name == testRole);

            Assert.Single(actualRole);
            Assert.Equal(testRole, actualRole.FirstOrDefault().Name);

            await identityService.DeleteRole(actualRole.FirstOrDefault().Id);

            var deletedRole = identityService.GetRoles(new System.Threading.CancellationToken()).Result.Where(u => u.Name == testRole);

            Assert.Empty(deletedRole);
        }

        [Theory]
        [InlineData(new string[] { "apitest1" }, "apitestrole1", 1)]
        [InlineData(new string[] { "apitest1", "apitest2", "apitest3" }, "apitestrole2", 3)]
        public async void AddRemoveUserFromRoleAsync(string[] testUsernNames, string testRole, int expectedCount)
        {
            await identityService.AddRole(testRole);
            var identityRole = identityService.GetRoles(new System.Threading.CancellationToken()).Result.Where(r => r.Name == testRole).FirstOrDefault();

            foreach (var testUserName in testUsernNames)
            {
                var user = new IdentityUser { UserName = testUserName };
                await identityService.AddUser(user, testUserName, new System.Threading.CancellationToken());

                var identityUser = identityService.GetUsers().Result.Where(u => u.UserName == testUserName).FirstOrDefault();

                var addResult = await identityService.AddUserToRole(testUserName, testRole);
                Assert.True(addResult.Succeeded);

                var getResult = await identityService.GetRolesForUser(testUserName);
                Assert.Equal(testRole, getResult.FirstOrDefault());
            }

            var getUsersInRoleResult = await identityService.GetUsersForRole(testRole);
            Assert.Equal(expectedCount, getUsersInRoleResult.Count());
            
            foreach (var testUserName in testUsernNames)
            {
                Assert.Contains(getUsersInRoleResult, u => u.UserName == testUserName);
                var removeResult = await identityService.RemoveRoleFromUser(testUserName, testRole);
                Assert.True(removeResult.Succeeded);

                var identityUser = identityService.GetUsers().Result.Where(u => u.UserName == testUserName).FirstOrDefault();

                var removeUserResult = await identityService.DeleteUser(identityUser.Id);
                Assert.True(removeUserResult.Succeeded);
            }
            var removeRoleResult = await identityService.DeleteRole(identityRole.Id);
            Assert.True(removeRoleResult.Succeeded);

        }
    }
}
