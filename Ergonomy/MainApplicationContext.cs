using Ergonomy.Logging;
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
        private ActivityMonitor _activityMonitor;
        private DataLogger _dataLogger;
        private System.Windows.Forms.Timer _activityTimer;
        private int _sessionCloseCounter = 0;
        private int _totalCloseCounter = 0;
        private bool _isAlarmActive = false;
        private NotifyIcon _notifyIcon;

        public MainApplicationContext()
        {
            LoadAppSettings();

            _activityMonitor = new ActivityMonitor();
            _activityMonitor.Start();

            _dataLogger = new DataLogger(_activityMonitor, () => _totalCloseCounter, _appSettings);
            _dataLogger.Start();

            _activityTimer = new System.Windows.Forms.Timer();
            _activityTimer.Interval = 1000;
            _activityTimer.Tick += OnActivityTimerTick;
            _activityTimer.Start();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Ergonomy";
            // The ContextMenuStrip and its "Exit" item have been removed to prevent closing from the tray icon.
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

        private void OnActivityTimerTick(object sender, EventArgs e)
        {
            if (_isAlarmActive) return;

            var totalActivity = _activityMonitor.AlarmKeyboardActiveTime + _activityMonitor.AlarmMouseActiveTime;
            if (totalActivity.TotalSeconds >= _appSettings.ActivityThresholdSeconds)
            {
                _activityMonitor.ResetAlarmTimers();
                ShowAlarm();
            }
        }

        private void ShowAlarm()
        {
            _isAlarmActive = true;

            if (_sessionCloseCounter < _appSettings.SessionCloseLimit)
            {
                var primaryAlarm = new PrimaryAlarmForm(_appSettings);
                primaryAlarm.FormClosedCallback += (isUserClose) => {
                    if (isUserClose)
                    {
                        _sessionCloseCounter++;
                        _totalCloseCounter++;
                    }
                    _isAlarmActive = false;
                };
                primaryAlarm.Show();
            }
            else
            {
                var secondaryAlarm = new SecondaryAlarmForm(_appSettings);
                secondaryAlarm.FormClosed += (s, args) => {
                    _sessionCloseCounter = 0;
                    _isAlarmActive = false;
                };
                secondaryAlarm.Show();
            }
        }

    }
}