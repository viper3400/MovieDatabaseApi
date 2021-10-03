using Jaxx.VideoDb.WebCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieDatabaseCLI
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {

            Console.WriteLine("Starting host ...");
            var host = CliHost.Host();
            
            await host.RunConsoleAsync();
            return Environment.ExitCode;

           
            //var digitalCopySync = host.Services.GetService(typeof(DigitalCopySync)) as DigitalCopySync;
            //Console.WriteLine("Start search ....");
            //System.Threading.Tasks.Task<System.Collections.Generic.List<string>> task = digitalCopySync.ScanDigitalCopies("V:\\Filme", "*.mkv");
            //Console.WriteLine("Writing file ....");
            //System.IO.File.AppendAllLines("D:\\result.txt", task.Result);
            //Console.WriteLine("Stopping host ....");
            //await host.StopAsync();
            //Console.WriteLine("Finished!");
        }
    }
}
