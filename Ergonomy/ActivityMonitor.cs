using Ergonomy.Hooks;
using System;

namespace Ergonomy
{
    public class ActivityMonitor : IDisposable
    {
        private GlobalInputHook _globalInputHook;
        private DateTime _lastKeyboardActivity;
        private DateTime _lastMouseActivity;

        public TimeSpan AlarmKeyboardActiveTime { get; private set; }
        public TimeSpan AlarmMouseActiveTime { get; private set; }
        public TimeSpan TotalKeyboardActiveTime { get; private set; }
        public TimeSpan TotalMouseActiveTime { get; private set; }

        public ActivityMonitor()
        {
            _globalInputHook = new GlobalInputHook();
            _globalInputHook.KeyboardActivity += OnKeyboardActivity;
            _globalInputHook.MouseActivity += OnMouseActivity;

            AlarmKeyboardActiveTime = TimeSpan.Zero;
            AlarmMouseActiveTime = TimeSpan.Zero;
            TotalKeyboardActiveTime = TimeSpan.Zero;
            TotalMouseActiveTime = TimeSpan.Zero;
        }

        public void Start()
        {
            _lastKeyboardActivity = DateTime.Now;
            _lastMouseActivity = DateTime.Now;
            _globalInputHook.Start();
        }

        public void Stop()
        {
            _globalInputHook.Stop();
        }

        public void ResetAlarmTimers()
        {
            AlarmKeyboardActiveTime = TimeSpan.Zero;
            AlarmMouseActiveTime = TimeSpan.Zero;
        }

        public void ResetTotalTimers()
        {
            TotalKeyboardActiveTime = TimeSpan.Zero;
            TotalMouseActiveTime = TimeSpan.Zero;
        }

        private void OnKeyboardActivity(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsed = now - _lastKeyboardActivity;
            if (elapsed.TotalSeconds < 1)
            {
                AlarmKeyboardActiveTime += elapsed;
                TotalKeyboardActiveTime += elapsed;
            }
            _lastKeyboardActivity = now;
        }

        private void OnMouseActivity(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsed = now - _lastMouseActivity;
            if (elapsed.TotalSeconds < 1)
            {
                AlarmMouseActiveTime += elapsed;
                TotalMouseActiveTime += elapsed;
            }
            _lastMouseActivity = now;
        }

        public void Dispose()
        {
            _globalInputHook.Dispose();
        }
    }
}