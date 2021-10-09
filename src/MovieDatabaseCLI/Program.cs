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

        }
    }
}
