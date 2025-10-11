// Import necessary namespaces for hooks and system functions.
using Ergonomy.Hooks;
using System;

// Define the main namespace for the application.
namespace Ergonomy
{
    // This class is responsible for tracking the user's keyboard and mouse activity.
    // It implements IDisposable to ensure the input hooks are released when the object is no longer needed.
    public class ActivityMonitor : IDisposable
    {
        // This object handles the low-level global input hooks.
        private GlobalInputHook _globalInputHook;
        // Stores the exact time of the last recorded keyboard activity.
        private DateTime _lastKeyboardActivity;
        // Stores the exact time of the last recorded mouse activity.
        private DateTime _lastMouseActivity;

        // Public property to get the accumulated keyboard time for the 5-second alarm. Resets after each alarm.
        public TimeSpan AlarmKeyboardActiveTime { get; private set; }
        // Public property to get the accumulated mouse time for the 5-second alarm. Resets after each alarm.
        public TimeSpan AlarmMouseActiveTime { get; private set; }
        // Public property to get the total accumulated keyboard time for the hourly log. Resets every hour.
        public TimeSpan TotalKeyboardActiveTime { get; private set; }
        // Public property to get the total accumulated mouse time for the hourly log. Resets every hour.
        public TimeSpan TotalMouseActiveTime { get; private set; }

        // This is the constructor. It runs when a new ActivityMonitor is created.
        public ActivityMonitor()
        {
            // Create a new GlobalInputHook object.
            _globalInputHook = new GlobalInputHook();
            // Subscribe the OnKeyboardActivity method to the KeyboardActivity event from the hook.
            _globalInputHook.KeyboardActivity += OnKeyboardActivity;
            // Subscribe the OnMouseActivity method to the MouseActivity event from the hook.
            _globalInputHook.MouseActivity += OnMouseActivity;

            // Initialize all time spans to zero.
            AlarmKeyboardActiveTime = TimeSpan.Zero;
            AlarmMouseActiveTime = TimeSpan.Zero;
            TotalKeyboardActiveTime = TimeSpan.Zero;
            TotalMouseActiveTime = TimeSpan.Zero;
        }

        // This method starts the activity monitoring.
        public void Start()
        {
            // Set the initial last activity time to the current time.
            _lastKeyboardActivity = DateTime.Now;
            _lastMouseActivity = DateTime.Now;
            // Start the low-level input hooks.
            _globalInputHook.Start();
        }

        // This method stops the activity monitoring.
        public void Stop()
        {
            // Stop the low-level input hooks.
            _globalInputHook.Stop();
        }

        // This method resets the timers used for the 5-second alarm.
        public void ResetAlarmTimers()
        {
            AlarmKeyboardActiveTime = TimeSpan.Zero;
            AlarmMouseActiveTime = TimeSpan.Zero;
        }

        // This method resets the timers used for the hourly data log.
        public void ResetTotalTimers()
        {
            TotalKeyboardActiveTime = TimeSpan.Zero;
            TotalMouseActiveTime = TimeSpan.Zero;
        }

        // This method is called every time a keyboard event is detected by the GlobalInputHook.
        private void OnKeyboardActivity(object sender, EventArgs e)
        {
            // Get the current time.
            var now = DateTime.Now;
            // Calculate the time that has passed since the last keyboard event.
            var elapsed = now - _lastKeyboardActivity;
            // To avoid counting long idle periods, only add the time if it's less than 1 second.
            if (elapsed.TotalSeconds < 1)
            {
                // Add the elapsed time to both the alarm counter and the total hourly counter.
                AlarmKeyboardActiveTime += elapsed;
                TotalKeyboardActiveTime += elapsed;
            }
            // Update the last activity time to the current time.
            _lastKeyboardActivity = now;
        }

        // This method is called every time a mouse event is detected by the GlobalInputHook.
        private void OnMouseActivity(object sender, EventArgs e)
        {
            // Get the current time.
            var now = DateTime.Now;
            // Calculate the time that has passed since the last mouse event.
            var elapsed = now - _lastMouseActivity;
            // To avoid counting long idle periods, only add the time if it's less than 1 second.
            if (elapsed.TotalSeconds < 1)
            {
                // Add the elapsed time to both the alarm counter and the total hourly counter.
                AlarmMouseActiveTime += elapsed;
                TotalMouseActiveTime += elapsed;
            }
            // Update the last activity time to the current time.
            _lastMouseActivity = now;
        }

        // This method is part of the IDisposable interface. It's called to clean up resources.
        public void Dispose()
        {
            // Dispose of the GlobalInputHook object, which will unhook it from the system.
            _globalInputHook.Dispose();
        }
    }
}