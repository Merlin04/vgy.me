using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using Microsoft.Extensions.DependencyInjection;

namespace vgy.me
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            var services = new ServiceCollection();
            // Add services
            services.AddHttpClient();
            services.AddSingleton<ConfigurationService>();
            // Add commands
            services.AddTransient<ConfigureCommand>();
            services.AddTransient<ConfigureResetCommand>();
            services.AddTransient<DeleteCommand>();
            services.AddTransient<UploadCommand>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTitle("vgy.me CLI")
                .UseDescription("Unofficial CLI for vgy.me")
                .UseExecutableName("vgy.me")
                .UseVersionText("v0.1.0")
                .UseTypeActivator(serviceProvider.GetService)
                .Build()
                .RunAsync();
        }
    }
}