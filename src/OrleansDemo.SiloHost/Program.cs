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

namespace OrleansDemo.SiloHost
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static int Main(string[] args)
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";
            var builder = new ConfigurationBuilder();
            if (isDevelopment) 
            {
                builder.AddUserSecrets<Program>();
            }
            Configuration = builder.Build();
            return RunMainAsync(args.Length > 0 ? Convert.ToInt32(args[0]) : 0).Result;
        }

        private static async Task<int> RunMainAsync(int portAdd)
        {
            try
            {
                var host = await StartSilo(portAdd);
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo(int portAdd)
        {
            var builder = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansDemo";
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = Configuration["Orleans:ClusterStorage"];
                })
                .AddAdoNetGrainStorage("Devices", options=>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = Configuration["Orleans:GrainStorage"];
                    options.UseJsonFormat = true;
                })
                .ConfigureEndpoints(siloPort: 11111 + portAdd, gatewayPort: 30000 + portAdd)
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                .UseDashboard(_ => {  })
                .UseLinuxEnvironmentStatistics()
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
