using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ergonomy.UI
{
    public partial class SecondaryAlarmForm : Form
    {
        private System.Windows.Forms.Timer _unclosableTimer;
        private System.Windows.Forms.Timer _autoCloseTimer;

        public SecondaryAlarmForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ControlBox = false; // Disable the close button

            _unclosableTimer = new System.Windows.Forms.Timer();
            _unclosableTimer.Interval = 10000; // 10 seconds
            _unclosableTimer.Tick += (sender, e) => {
                this.ControlBox = true; // Enable the close button
                _unclosableTimer.Stop();
                _autoCloseTimer.Start();
            };
            _unclosableTimer.Start();

            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = 7000; // 7 seconds
            _autoCloseTimer.Tick += (sender, e) => {
                this.Close();
            };
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _unclosableTimer.Stop();
            _unclosableTimer.Dispose();
            _autoCloseTimer.Stop();
            _autoCloseTimer.Dispose();
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // label1
            //
            this.label1 = new System.Windows.Forms.Label();
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("B Nazanin", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label1.Size = new System.Drawing.Size(600, 84);
            this.label1.TabIndex = 0;
            this.label1.Text = "شما 3 بار نرمش را نادیده گرفتید، این پنجره به مدت 10 ثانیه قابلیت بسته شدن ندارد";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // SecondaryAlarmForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 102);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SecondaryAlarmForm";
            this.Text = "هشدار";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label label1;
    }
}