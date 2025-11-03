using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ergonomy
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            else
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainApplicationContext(args));
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ErgonomyService>();
                });
    }
}
