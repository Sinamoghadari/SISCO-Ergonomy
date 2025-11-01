using Ergonomy.Hooks;
using Ergonomy.Logging;
using Ergonomy.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Ergonomy
{
    public class MainApplicationContext : ApplicationContext
    {
        private AppSettings _appSettings;
        private System.Windows.Forms.Timer _notificationTimer;
        private int _sessionCloseCounter = 0;
        private int _totalCloseCounter = 0;
        private bool _isAlarmActive = false;
        private NotifyIcon _notifyIcon;
        private List<string> _imagePaths;
        private int _currentImageIndex = 0;
        private GlobalInputHook _globalInputHook;
        private ActivityMonitor _activityMonitor;
        private DataLogger _dataLogger;

        public MainApplicationContext()
        {
            LoadAppSettings();
            LoadImagePaths();

            _globalInputHook = new GlobalInputHook();
            _activityMonitor = new ActivityMonitor(_globalInputHook);
            _dataLogger = new DataLogger(_activityMonitor, () => _totalCloseCounter, _appSettings);
            _activityMonitor.Start();
            _dataLogger.Start();

            _notificationTimer = new System.Windows.Forms.Timer();
            _notificationTimer.Interval = _appSettings.NotificationIntervalSeconds * 1000;
            _notificationTimer.Tick += OnNotificationTimerTick;
            _notificationTimer.Start();

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

        private void LoadImagePaths()
        {
            var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            if (Directory.Exists(assetsPath))
            {
                _imagePaths = Directory.GetFiles(assetsPath, "*.png")
                    .Concat(Directory.GetFiles(assetsPath, "*.gif"))
                    .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)))
                    .ToList();
            }
            else
            {
                _imagePaths = new List<string>();
            }
        }

        private void OnNotificationTimerTick(object sender, EventArgs e)
        {
            if (_isAlarmActive) return;

            ShowAlarm();
        }

        private void ShowAlarm()
        {
            _isAlarmActive = true;

            string imagePath = null;
            if (_imagePaths.Count > 0)
            {
                imagePath = _imagePaths[_currentImageIndex];
                _currentImageIndex = (_currentImageIndex + 1) % _imagePaths.Count;
            }

            if (_sessionCloseCounter < _appSettings.SessionCloseLimit)
            {
                var primaryAlarm = new PrimaryAlarmForm(_appSettings, imagePath);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activityMonitor.Dispose();
                _dataLogger.Dispose();
                _globalInputHook.Dispose();
                _notificationTimer.Dispose();
                _notifyIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}