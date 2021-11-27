using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace OrleansDemo.WebApp
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private IClusterClient _client;
        public IConfiguration Configuration { get; }

        public Startup(ILogger<Startup> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(CreateClusterClient);
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            _client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansDemo";
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = Configuration.GetConnectionString("ClusterStorage");
                })                
                .ConfigureLogging(_ => _.AddConsole())
                .Build();
           
            StartClientWithRetries(_client).Wait();
            return _client;
        }

        private async Task StartClientWithRetries(IClusterClient client)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    await client.Connect();
                    _logger.LogInformation("Connected to the Orleans' cluster.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting Orleans client");
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        private void OnShutdown()
        {
            if(_client != null)
            {
                _logger.LogInformation("Closing Orleans client...");
                _client.Close();
                _logger.LogInformation("Orleans client closed.");
            }
        }
    }
}