using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using VM_IP_VOD_CP_File_Manager.Application;
using VM_IP_VOD_CP_File_Manager.BusinessLogic;
using VM_IP_VOD_CP_File_Manager.BusinessLogic.Contracts;
using VM_IP_VOD_CP_File_Manager.Service;

namespace VM_IP_VOD_CP_File_Manager
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            //set to ensure serilog uses working directory not project root.
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .CreateLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.Warning("Service Stopping.");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) => {
                    services
                        .AddOptions()
                        .AddScoped<IWorkflowProcessor, WorkflowProcessor>()
                        .Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"))
                        .AddHostedService<Worker>();
                })
                .UseSerilog((hostingContext, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration));
        
    }
}
