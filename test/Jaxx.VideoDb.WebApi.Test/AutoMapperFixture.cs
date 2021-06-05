using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class AutoMapperFixture : IDisposable
    {
        private static readonly object Sync = new object();
        private static bool _configured;

        public AutoMapperFixture()
        {
        // Not clear, if this class is needed any longer, but Mapper.Initalize provoked an Obsolete warning
        // Severity Code    Description Project File Line    Suppression State
        // Warning CS0618  'Mapper.Initialize(Action<IMapperConfigurationExpression>)' is obsolete: 
        // 'Switch to the instance based API, preferably using dependency injection. 
        // See http://docs.automapper.org/en/latest/Static-and-Instance-API.html and http://docs.automapper.org/en/latest/Dependency-injection.html.'	
        // Jaxx.VideoDb.WebApi.Test D:\WORK\jaxx.net.videodb.api\test\Jaxx.VideoDb.WebApi.Test\AutoMapperFixture.cs 23  Active


            //lock (Sync)
            //{
            //    if (!_configured)
            //    {
            //        Mapper.Initialize(cfg =>
            //        {
            //            cfg.AddProfile(new Infrastructure.DefaultAutomapperProfile());
            //        });
            //    }
            //    _configured = true;
            //}
        }

        public void Dispose()
        {
            // ... clean up test data
        }
    }
}