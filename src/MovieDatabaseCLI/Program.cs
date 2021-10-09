using CommandLine;
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

        static int Main(string[] args)
        {

            Parser.Default.ParseArguments<OrphanFilesOptions, CheckFilesExistsOptions, FindMatchesOptions>(args)
            .WithParsed<OrphanFilesOptions>(async options =>
            {
                var host = CliHost.Host();
                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(options);
                    services.AddHostedService<OrphanFileWorker>();
                });
                await host.RunConsoleAsync();
            })
            .WithParsed<CheckFilesExistsOptions>(async options => 
            {
                var host = CliHost.Host();
                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(options);
                    services.AddHostedService<CheckFileExistsWorker>();
                });
                await host.RunConsoleAsync();
            }).WithParsed<FindMatchesOptions>(async options =>
            {
                var host = CliHost.Host();
                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(options);
                    services.AddHostedService<FindMatchesWoker>();
                });
                await host.RunConsoleAsync();
            });

            //.WithNotParsed(errors => Console.WriteLine(errors));

            return Environment.ExitCode;


            /*Console.WriteLine("Starting host ...");
             var host = CliHost.Host().ConfigureServices((hostContext, services) => 

             services.AddHostedService<Worker>()) ;

             await host.RunConsoleAsync();*/



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
