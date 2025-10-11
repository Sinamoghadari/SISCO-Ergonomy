// Import necessary namespaces for Excel handling (EPPlus), system functions, globalization (for calendars), file IO, and timers.
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Timers;

// Define the namespace for logging-related classes.
namespace Ergonomy.Logging
{
    // This class handles the creation of hourly Excel log files.
    // It implements IDisposable to ensure the timer is properly released.
    public class DataLogger : IDisposable
    {
        // A timer that fires an event every hour to trigger the logging process.
        private System.Timers.Timer _logTimer;
        // A reference to the ActivityMonitor to get the keyboard and mouse activity data.
        private ActivityMonitor _activityMonitor;
        // A function (passed in from the MainApplicationContext) that returns the current total close counter.
        private Func<int> _getTotalCloseCounter;

        // This is the constructor. It's called when a new DataLogger is created.
        public DataLogger(ActivityMonitor activityMonitor, Func<int> getTotalCloseCounter)
        {
            // Store the provided ActivityMonitor object.
            _activityMonitor = activityMonitor;
            // Store the provided function for getting the close counter.
            _getTotalCloseCounter = getTotalCloseCounter;

            // Create a new Timer object.
            // 3600000 milliseconds = 1 hour.
            _logTimer = new System.Timers.Timer(3600000);
            // Tell the timer to call the OnLogTimerElapsed method every time it fires.
            _logTimer.Elapsed += OnLogTimerElapsed;
        }

        // This method starts the hourly logging timer.
        public void Start()
        {
            _logTimer.Start();
        }

        // This method stops the hourly logging timer.
        public void Stop()
        {
            _logTimer.Stop();
        }

        // This method is called every hour by the _logTimer.
        private void OnLogTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Call the main method to perform the data logging.
            LogData();
        }

        // This method contains the logic for creating and saving the Excel file.
        private void LogData()
        {
            // Use a try-catch block to handle potential errors during file operations.
            try
            {
                // Get the time zone information for Tehran ("Iran Standard Time").
                var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                // Convert the current universal time (UTC) to Tehran's local time.
                var tehranTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tehranTimeZone);
                // Create an instance of the PersianCalendar to convert the date.
                var persianCalendar = new PersianCalendar();

                // Format the file name using the Shamsi date and Tehran time.
                // Example: 1404-07-09_22-45.xlsx
                var fileName = string.Format("{0:0000}-{1:00}-{2:00}_{3:00}-{4:00}.xlsx",
                    persianCalendar.GetYear(tehranTime),
                    persianCalendar.GetMonth(tehranTime),
                    persianCalendar.GetDayOfMonth(tehranTime),
                    tehranTime.Hour,
                    tehranTime.Minute);

                // Combine the application's base directory with the new file name to get the full path.
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                // Set the license context for EPPlus. This is required for non-commercial use.
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Create a new Excel package object. The 'using' statement ensures it's properly disposed of.
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Add a new worksheet to the Excel file named "ActivityLog".
                    var worksheet = package.Workbook.Worksheets.Add("ActivityLog");
                    // Set the header text for the first column.
                    worksheet.Cells[1, 1].Value = "Keyboard Activity (s)";
                    // Set the header text for the second column.
                    worksheet.Cells[1, 2].Value = "Mouse Activity (s)";
                    // Set the header text for the third column.
                    worksheet.Cells[1, 3].Value = "Total Close Counter";

                    // Get the total keyboard activity (in seconds) and put it in the second row, first column.
                    worksheet.Cells[2, 1].Value = _activityMonitor.TotalKeyboardActiveTime.TotalSeconds;
                    // Get the total mouse activity (in seconds) and put it in the second row, second column.
                    worksheet.Cells[2, 2].Value = _activityMonitor.TotalMouseActiveTime.TotalSeconds;
                    // Get the total close counter value and put it in the second row, third column.
                    worksheet.Cells[2, 3].Value = _getTotalCloseCounter();

                    // Save the Excel file to disk.
                    package.Save();
                }

                // After saving the data, reset the total activity timers in the monitor.
                _activityMonitor.ResetTotalTimers();
            }
            catch (Exception ex)
            {
                // If an error occurs (e.g., cannot write the file), print the error message to the console.
                // In a real application, this might be written to a text log file instead.
                Console.WriteLine("Error logging data: " + ex.Message);
            }
        }

        // This method is part of the IDisposable interface. It's called to clean up resources.
        public void Dispose()
        {
            // Dispose of the timer object.
            _logTimer.Dispose();
        }
    }
}