using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ergonomy.UI
{
    public partial class PrimaryAlarmForm : Form
    {
        public event Action<bool> FormClosedCallback;
        private System.Windows.Forms.Timer _autoCloseTimer;

        public PrimaryAlarmForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);

            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = 7000; // 7 seconds
            _autoCloseTimer.Tick += (sender, e) => {
                FormClosedCallback?.Invoke(false); // Auto-closed
                this.Close();
            };
            _autoCloseTimer.Start();
        }

        private bool _isUserClose = false;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                _isUserClose = true;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _autoCloseTimer.Stop();
            _autoCloseTimer.Dispose();
            FormClosedCallback?.Invoke(_isUserClose);
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
            this.label1.Size = new System.Drawing.Size(250, 42);
            this.label1.TabIndex = 0;
            this.label1.Text = "زمان یک نرمش فرا رسیده است";
            //
            // pictureBox1
            //
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox1.Location = new System.Drawing.Point(12, 54);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(250, 150);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Image = Image.FromFile("assets/exercise.gif");
            //
            // PrimaryAlarmForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 216);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PrimaryAlarmForm";
            this.Text = "استراحت";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}