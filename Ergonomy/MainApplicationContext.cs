using Ergonomy.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows.Forms;

namespace Ergonomy
{
    public class MainApplicationContext : ApplicationContext
    {
        private AppSettings _appSettings;

        public MainApplicationContext(string[] args)
        {
            LoadAppSettings();

            if (args.Length > 0)
            {
                string formToOpen = args[0];
                string imagePath = args.Length > 1 ? args[1] : null;

                if (formToOpen == "primary")
                {
                    var primaryAlarm = new PrimaryAlarmForm(_appSettings, imagePath);
                    primaryAlarm.FormClosed += (s, e) => Application.Exit();
                    primaryAlarm.Show();
                }
                else if (formToOpen == "secondary")
                {
                    var secondaryAlarm = new SecondaryAlarmForm(_appSettings);
                    secondaryAlarm.FormClosed += (s, e) => Application.Exit();
                    secondaryAlarm.Show();
                }
            }
        }

        private void LoadAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            _appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(_appSettings);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
