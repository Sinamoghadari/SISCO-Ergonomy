// Import necessary namespaces for logging, UI, system functions, and Windows Forms.
using Ergonomy.Logging;
using Ergonomy.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows.Forms;

// Define the main namespace for the application.
namespace Ergonomy
{
    // This class manages the entire application's lifecycle. It doesn't have a visible window.
    // It inherits from ApplicationContext, which is ideal for background applications.
    public class MainApplicationContext : ApplicationContext
    {
        // This property will hold all the loaded application settings.
        private AppSettings _appSettings;
        // This object is responsible for monitoring keyboard and mouse activity.
        private ActivityMonitor _activityMonitor;
        // This object handles logging the activity data to an Excel file.
        private DataLogger _dataLogger;
        // This timer checks for user activity every second.
        private System.Windows.Forms.Timer _activityTimer;
        // This counter tracks how many times the user has closed the primary alarm in the current session.
        private int _sessionCloseCounter = 0;
        // This counter tracks the total number of times the user has ever closed the primary alarm.
        private int _totalCloseCounter = 0;
        // This flag prevents multiple alarms from showing up at the same time.
        private bool _isAlarmActive = false;
        // This is the icon that appears in the system tray (the area by the clock).
        private NotifyIcon _notifyIcon;

        // This is the constructor. It runs once when the application starts.
        public MainApplicationContext()
        {
            // Load the application settings from the JSON file first.
            LoadAppSettings();

            // Create a new ActivityMonitor object.
            _activityMonitor = new ActivityMonitor();
            // Start monitoring for keyboard and mouse activity.
            _activityMonitor.Start();

            // Create a new DataLogger object.
            // It needs the activity monitor to get data, a function to get the total close count, and the app settings.
            _dataLogger = new DataLogger(_activityMonitor, () => _totalCloseCounter, _appSettings);
            // Start the hourly logging timer.
            _dataLogger.Start();

            // Create a new Timer object.
            _activityTimer = new System.Windows.Forms.Timer();
            // Set the timer to fire every 1000 milliseconds (1 second).
            _activityTimer.Interval = 1000;
            // Tell the timer to call the OnActivityTimerTick method every time it fires.
            _activityTimer.Tick += OnActivityTimerTick;
            // Start the timer.
            _activityTimer.Start();

            // Create the system tray icon.
            _notifyIcon = new NotifyIcon();
            // Set the icon to a default system icon.
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            // Make the icon visible.
            _notifyIcon.Visible = true;
            // Set the text that appears when you hover over the icon.
            _notifyIcon.Text = "Ergonomy";
            // Create a right-click menu for the icon.
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            // Add an "Exit" button to the menu. When clicked, it calls the OnExit method.
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, OnExit);
        }

        // This method builds the configuration from the appsettings.json file.
        private void LoadAppSettings()
        {
            // Create a new configuration builder.
            var builder = new ConfigurationBuilder()
                // Set the base path to the application's directory.
                .SetBasePath(Directory.GetCurrentDirectory())
                // Add the appsettings.json file as a source.
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Build the configuration.
            IConfigurationRoot configuration = builder.Build();

            // Create a new instance of our AppSettings class.
            _appSettings = new AppSettings();
            // Bind the "AppSettings" section of the JSON file to our AppSettings object.
            configuration.GetSection("AppSettings").Bind(_appSettings);
        }

        // This method is called every second by the _activityTimer.
        private void OnActivityTimerTick(object sender, EventArgs e)
        {
            // If an alarm window is already being shown, do nothing and exit the method.
            if (_isAlarmActive)
            {
                return;
            }

            // Calculate the total combined activity time from the monitor.
            var totalActivity = _activityMonitor.AlarmKeyboardActiveTime + _activityMonitor.AlarmMouseActiveTime;
            // If the total activity reaches the threshold from our settings...
            if (totalActivity.TotalSeconds >= _appSettings.ActivityThresholdSeconds)
            {
                // Reset the timers used for the alarm.
                _activityMonitor.ResetAlarmTimers();
                // Call the method to show an alarm window.
                ShowAlarm();
            }
        }

        // This method shows the appropriate alarm window based on the session close counter.
        private void ShowAlarm()
        {
            // Set the flag to true so we know an alarm is active.
            _isAlarmActive = true;

            // If the user has closed the primary alarm less than the limit from our settings...
            if (_sessionCloseCounter < _appSettings.SessionCloseLimit)
            {
                // Create the primary alarm window, passing the settings to it.
                var primaryAlarm = new PrimaryAlarmForm(_appSettings);
                // Set up a callback that runs when the form is closed.
                primaryAlarm.FormClosedCallback += (isUserClose) => {
                    // --- DIAGNOSTIC LOG ---
                    Console.WriteLine($"DIAGNOSTIC: Callback received. isUserClose = {isUserClose}. Current session counter = {_sessionCloseCounter}");

                    // This is the crucial part of the logic.
                    if (isUserClose)
                    {
                        // Therefore, this counter only increments on a manual user close.
                        _sessionCloseCounter++;
                        _totalCloseCounter++;
                        // --- DIAGNOSTIC LOG ---
                        Console.WriteLine($"DIAGNOSTIC: User closed form. New session counter = {_sessionCloseCounter}");
                    }
                    // Reset the flag because the alarm is now closed, allowing a new one to appear later.
                    _isAlarmActive = false;
                };
                // Show the primary alarm window.
                primaryAlarm.Show();
            }
            else // Otherwise (if the counter is at the limit)...
            {
                // Create the secondary (escalation) alarm window, passing the settings to it.
                var secondaryAlarm = new SecondaryAlarmForm(_appSettings);
                // Set up a callback that runs when the form is closed.
                secondaryAlarm.FormClosed += (s, args) => {
                    // Reset the session counter back to zero.
                    _sessionCloseCounter = 0;
                    // Reset the flag because the alarm is now closed.
                    _isAlarmActive = false;
                };
                // Show the secondary alarm as a dialog, which blocks interaction with other windows.
                secondaryAlarm.ShowDialog();
            }
        }

        // This method is called when the user clicks "Exit" from the tray icon menu.
        private void OnExit(object sender, EventArgs e)
        {
            // This tells the application to start shutting down gracefully.
            ExitThread();
        }

        // This method is called when the application is exiting.
        protected override void ExitThreadCore()
        {
            // Hide the tray icon so it doesn't linger after the app closes.
            _notifyIcon.Visible = false;
            // Stop the activity monitor.
            _activityMonitor.Stop();
            // Stop the data logger.
            _dataLogger.Stop();
            // Stop the activity timer.
            _activityTimer.Stop();
            // Call the base method to complete the shutdown process.
            base.ExitThreadCore();
        }
    }
}