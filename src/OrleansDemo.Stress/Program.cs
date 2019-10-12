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
using OrleansDemo.Contracts;

namespace OrleansDemo.Stress
{
    class Program
    {
        private static IConfiguration _config;

        public static async Task Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets<Program>()
                .Build();

            long limit = long.Parse(args[0]);
            IClusterClient client = CreateClusterClient();
            for(long i = 0; i < limit; i++)
            {
                var device = client.GetGrain<IDevice>(i);
                await device.GetTemperature();
                Console.WriteLine(i);
            }
        }

        private static IClusterClient CreateClusterClient()
        {
            IClusterClient client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansDemo";
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = _config.GetConnectionString("ClusterStorage");
                })                
                .ConfigureLogging(_ => _.AddConsole())
                .Build();
           
            StartClientWithRetries(client).Wait();
            return client;
        }

        private static async Task StartClientWithRetries(IClusterClient client)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    await client.Connect();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting Orleans client" + ex.ToString());
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
