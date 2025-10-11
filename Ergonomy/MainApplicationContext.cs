using Ergonomy.Logging;
using Ergonomy.UI;
using System;
using System.Windows.Forms;

namespace Ergonomy
{
    public class MainApplicationContext : ApplicationContext
    {
        private ActivityMonitor _activityMonitor;
        private DataLogger _dataLogger;
        private System.Windows.Forms.Timer _activityTimer;
        private int _sessionCloseCounter = 0;
        private int _totalCloseCounter = 0;
        private NotifyIcon _notifyIcon;

        public MainApplicationContext()
        {
            _activityMonitor = new ActivityMonitor();
            _activityMonitor.Start();

            _dataLogger = new DataLogger(_activityMonitor, () => _totalCloseCounter);
            _dataLogger.Start();

            _activityTimer = new System.Windows.Forms.Timer();
            _activityTimer.Interval = 1000; // 1 second
            _activityTimer.Tick += OnActivityTimerTick;
            _activityTimer.Start();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Ergonomy";
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, OnExit);
        }

        private void OnActivityTimerTick(object sender, EventArgs e)
        {
            var totalActivity = _activityMonitor.AlarmKeyboardActiveTime + _activityMonitor.AlarmMouseActiveTime;
            if (totalActivity.TotalSeconds >= 5)
            {
                _activityMonitor.ResetAlarmTimers();
                ShowAlarm();
            }
        }

        private void ShowAlarm()
        {
            if (_sessionCloseCounter < 3)
            {
                var primaryAlarm = new PrimaryAlarmForm();
                primaryAlarm.FormClosedCallback += (isUserClose) => {
                    if (isUserClose)
                    {
                        _sessionCloseCounter++;
                        _totalCloseCounter++;
                    }
                };
                primaryAlarm.Show();
            }
            else
            {
                var secondaryAlarm = new SecondaryAlarmForm();
                secondaryAlarm.FormClosed += (s, args) => {
                    _sessionCloseCounter = 0;
                };
                secondaryAlarm.ShowDialog();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _notifyIcon.Visible = false;
            _activityMonitor.Stop();
            _dataLogger.Stop();
            _activityTimer.Stop();
            base.ExitThreadCore();
        }
    }
}