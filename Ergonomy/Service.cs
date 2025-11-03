using Ergonomy.Hooks;
using Ergonomy.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ergonomy
{
    public class ErgonomyService : BackgroundService
    {
        private AppSettings _appSettings;
        private int _sessionCloseCounter = 0;
        private int _totalCloseCounter = 0;
        private List<string> _imagePaths;
        private int _currentImageIndex = 0;
        private GlobalInputHook _globalInputHook;
        private ActivityMonitor _activityMonitor;
        private DataLogger _dataLogger;
        private Process? _uiProcess;

        public ErgonomyService()
        {
            LoadAppSettings();
            LoadImagePaths();

            _globalInputHook = new GlobalInputHook();
            _activityMonitor = new ActivityMonitor(_globalInputHook);
            _dataLogger = new DataLogger(_activityMonitor, () => _totalCloseCounter, _appSettings);
        }

        private void LoadAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            _appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(_appSettings);
        }

        private void LoadImagePaths()
        {
            var assetsPath = Path.Combine(AppContext.BaseDirectory, "assets");
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _activityMonitor.Start();
            _dataLogger.Start();

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_appSettings.NotificationIntervalSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                ShowAlarm();
            }

            _activityMonitor.Dispose();
            _dataLogger.Dispose();
            _globalInputHook.Dispose();
        }

        private void ShowAlarm()
        {
            if (_uiProcess != null && !_uiProcess.HasExited)
            {
                return;
            }

            string imagePath = string.Empty;
            if (_imagePaths.Count > 0)
            {
                imagePath = _imagePaths[_currentImageIndex];
                _currentImageIndex = (_currentImageIndex + 1) % _imagePaths.Count;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "Ergonomy.exe"),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            if (_sessionCloseCounter < _appSettings.SessionCloseLimit)
            {
                processStartInfo.Arguments = $"primary \"{imagePath}\"";
            }
            else
            {
                processStartInfo.Arguments = "secondary";
            }

            var process = Process.Start(processStartInfo);
            if (process != null)
            {
                process.EnableRaisingEvents = true;
                process.Exited += OnUiProcessExited;
                _uiProcess = process;
            }
        }

        private void OnUiProcessExited(object? sender, EventArgs e)
        {
            if (sender is Process process)
            {
                if (process.StartInfo.Arguments.StartsWith("primary"))
                {
                    if (process.ExitCode == 1)
                    {
                        _sessionCloseCounter++;
                        _totalCloseCounter++;
                    }
                }
                else if (process.StartInfo.Arguments.StartsWith("secondary"))
                {
                    _sessionCloseCounter = 0;
                }

                process.Dispose();
                _uiProcess = null;
            }
        }
    }
}
