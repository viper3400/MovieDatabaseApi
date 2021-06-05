using Jaxx.VideoDb.Data.Context.ContextValidation;
using Jaxx.VideoDb.Data.MySql;
using Jaxx.WebApi.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class ValidationOnlineShould
    {
        private VideoDb.Data.Context.VideoDbContext _context;
        private readonly string _userName;
        private readonly string _viewGroup;

        public ValidationOnlineShould()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("ClientSecrets.json")
                .AddJsonFile("testsettings.json")
                .Build();

            _userName = config["TestUserName"];
            _viewGroup = config["TestViewGroup"];

            var connectionString = config["TESTDB_CONNECTIONSTRING"];
            _context = VideoDbContextFactory.Create(connectionString, new DummyUserContextInformationProvider(_userName, _viewGroup));
        }

        [Fact]
        [Trait("Category", "Online")]
        public void ValidateDiskIdIsInUse ()
        {
            var diskid = "R01F1D03";
            var validator = new DiskIdContextValidation(_context);
            var isUsed = validator.IsDiskIdUsed(diskid);
            Assert.True(isUsed);
        }

        [Fact]
        [Trait("Category", "Online")]
        public void ValidateDiskIdIsNotInUse()
        {
            var diskid = "R29F1D03";
            var validator = new DiskIdContextValidation(_context);
            var isUsed = validator.IsDiskIdUsed(diskid);
            Assert.False(isUsed);
        }
    }
}
