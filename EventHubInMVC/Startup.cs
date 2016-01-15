using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace EventHubInMVC
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            //////////////////////////////////////
            // "Force" HTTPS connection mode
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Https;
            //////////////////////////////////////
            // Custom messenger based on EventHub
            services.AddScoped(typeof(IMessenger), sp =>
            {
                //////////////////////////////////////
                // Put your credentials here
                var eventHubClient = EventHubClient.CreateFromConnectionString("YOUR_CONNECTION_STRING_HERE", "YOUR_EVENTHUB_NAME_HERE");
                //////////////////////////////////////
                // Change it to true to see a working case
                var closeOnDispose = false; 

                return new EventHubMessenger(eventHubClient, closeOnDispose, sp.GetService<ILogger<EventHubMessenger>>());
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseIISPlatformHandler();
            app.UseStaticFiles();

            //////////////////////////////////////
            // Custom middleware: send an event to the EH after request
            // have been processed
            app.UseMiddleware<TrackerMiddleWare>();



            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
