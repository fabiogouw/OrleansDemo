using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDemo.SiloHost
{
    public class SiloHostService : IHostedService
    {
        private IApplicationLifetime _appLifetime;
        private ILogger<SiloHostService> _logger;
        private IConfiguration _config;
        private ISiloHost _host;

        public SiloHostService(IApplicationLifetime appLifetime,
            ILogger<SiloHostService> logger,
            IConfiguration config)
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }        

        private void OnStarted()
        {
            try
            {
                int portAdd = _config.GetValue<int>("portadd");
                StartSilo(portAdd).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while instantiating silo.");
            }
        }

        private void OnStopping()
        {
            _logger.LogInformation("Stopping host...");
            if (_host != null)
            {
                _host.StopAsync().Wait();
            }
        }

        private void OnStopped()
        {
            _logger.LogInformation("Host stopped.");
        }        

        private async Task StartSilo(int portAdd)
        {
            var builder = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansDemo";
                })
                .Configure<GrainCollectionOptions>(options =>
                {
                    options.CollectionAge = TimeSpan.FromMinutes(3);
                    options.CollectionQuantum = TimeSpan.FromMinutes(2);
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = _config.GetConnectionString("ClusterStorage");
                })
                //.AddMemoryGrainStorage("Devices")
                .AddAdoNetGrainStorage("Devices", options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = _config.GetConnectionString("GrainStorage");
                    options.UseJsonFormat = true;
                })
                .ConfigureEndpoints(siloPort: 11111 + portAdd, gatewayPort: 30000 + portAdd)
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                .UseDashboard(options => options.Port = 8080 + portAdd)
                .UseLinuxEnvironmentStatistics()
                .ConfigureLogging(logging => logging.AddConsole());

            _host = builder.Build();
            await _host.StartAsync();
            _logger.LogInformation("Host started.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Host stopping...");
            if(_host != null)
            {
                _host.StopAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}
