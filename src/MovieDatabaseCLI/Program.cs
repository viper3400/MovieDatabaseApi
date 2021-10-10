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

            Parser.Default.ParseArguments<OrphanFilesOptions, CheckFilesExistsOptions, FindMatchesOptions, Line2ClipOptions, EntriesWithoutFilenameOptions, EntriesWithSameFilenameOptions>(args)
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
                    services.AddHostedService<FindMatchesWorker>();
                });
                await host.RunConsoleAsync();
            })
            .WithParsed<EntriesWithoutFilenameOptions>(async options =>
            {
                var host = CliHost.Host();
                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(options);
                    services.AddHostedService<EntriesWithoutFileWorker>();
                });
                await host.RunConsoleAsync();
            })
            .WithParsed<EntriesWithSameFilenameOptions>(async options =>
            {
                var host = CliHost.Host();
                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(options);
                    services.AddHostedService<EntriesWithSameFilenameWorker>();
                });
                await host.RunConsoleAsync();
            })
            .WithParsed<Line2ClipOptions>(options =>
            {
                var l2c = new Line2Clip(options.Input);
                l2c.Start();
            });

            //.WithNotParsed(errors => Console.WriteLine(errors));

            return Environment.ExitCode;

        }
    }
}
