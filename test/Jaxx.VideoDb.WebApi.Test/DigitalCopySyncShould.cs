using Jaxx.VideoDb.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class DigitalCopySyncShould
    {
        private readonly DigitalCopySync digitalCopySync;

        public DigitalCopySyncShould()
        {
            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            digitalCopySync = host.Services.GetService(typeof(DigitalCopySync)) as DigitalCopySync;
        }
        
        [Fact]
        public async void ScanDigitalCopies()
        {
            var result = await digitalCopySync.ScanDigitalCopies("V:\\Filme", "*.mkv");
        
        }
    }
}
