using System;
using System.Net;
using System.Threading.Tasks;
using Orleans.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Statistics;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace OrleansDemo.SiloHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddCommandLine(args);
                 })
                 .ConfigureAppConfiguration((hostContext, configBuilder) =>
                 {
                     configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.json", true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true)
                        .AddUserSecrets<Program>()
                        .AddCommandLine(args);
                 })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                })
                .UseOrleans((hostContext, orleansBuilder) =>
                {
                    var config = hostContext.Configuration;
                    int portAdd = config.GetValue<int>("portadd", 0);
                    orleansBuilder.Configure<ClusterOptions>(options =>
                     {
                         options.ClusterId = "dev";
                         options.ServiceId = "OrleansDemo";
                     })
                    .Configure<GrainCollectionOptions>(options =>
                    {
                        options.CollectionAge = TimeSpan.FromMinutes(3);
                        options.CollectionQuantum = TimeSpan.FromMinutes(2);
                    })
                    //.UseLocalhostClustering()
                    .UseAdoNetClustering(options =>
                    {
                        options.Invariant = "System.Data.SqlClient";
                        options.ConnectionString = config.GetConnectionString("ClusterStorage");
                    })
                    //.AddMemoryGrainStorage("Devices")
                    .AddAdoNetGrainStorage("Devices", options =>
                    {
                        options.Invariant = "System.Data.SqlClient";
                        options.ConnectionString = config.GetConnectionString("GrainStorage");
                        options.UseJsonFormat = true;
                    })
                    .ConfigureEndpoints(siloPort: 11111 + portAdd, gatewayPort: 30000 + portAdd)
                    //.ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                    .UseDashboard(options => options.Port = 8080 + portAdd)
                    //.UseLinuxEnvironmentStatistics()
                    .ConfigureLogging(logging => logging.AddConsole());
                })
                .Build();
            await host.RunAsync();
        }
    }
}
