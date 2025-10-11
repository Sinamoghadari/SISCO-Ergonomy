// Define the namespace for the application.
namespace Ergonomy
{
    // This class is a container for all the application settings.
    // Its properties match the keys in the "AppSettings" section of the appsettings.json file.
    // This allows the configuration system to automatically map the JSON values to these properties.
    public class AppSettings
    {
        // The number of seconds of continuous activity required to trigger the primary alarm.
        public int ActivityThresholdSeconds { get; set; }

        // The number of seconds the primary alarm will stay on screen before closing automatically.
        public int PrimaryAlarmAutoCloseSeconds { get; set; }

        // The number of times the user must manually close the primary alarm to trigger the secondary alarm.
        public int SessionCloseLimit { get; set; }

        // The number of seconds the secondary alarm remains unclosable.
        public int SecondaryAlarmUnclosableSeconds { get; set; }

        // The number of seconds the secondary alarm stays on screen (after becoming closable) before closing automatically.
        public int SecondaryAlarmAutoCloseSeconds { get; set; }

        // The interval in hours for logging activity data to the Excel file.
        public int LoggingIntervalHours { get; set; }
    }
}