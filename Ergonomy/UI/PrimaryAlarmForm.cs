using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ergonomy.UI
{
    public partial class PrimaryAlarmForm : Form
    {
        public event Action<bool> FormClosedCallback;
        private System.Windows.Forms.Timer _autoCloseTimer;
        private bool _isAutoClosing = false;

        public PrimaryAlarmForm(AppSettings settings)
        {
            InitializeComponent();
            this.TopMost = true;
            this.Resize += new System.EventHandler(this.AlarmForm_Resize);
            this.StartPosition = FormStartPosition.Manual;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);

            LoadRandomImage();

            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = settings.PrimaryAlarmAutoCloseSeconds * 1000;
            _autoCloseTimer.Tick += (sender, e) => {
                _isAutoClosing = true;
                this.Close();
            };
            _autoCloseTimer.Start();
        }

        private void LoadRandomImage()
        {
            var imageFiles = new System.Collections.Generic.List<string>();
            imageFiles.AddRange(System.IO.Directory.GetFiles("assets", "*.png"));
            imageFiles.AddRange(System.IO.Directory.GetFiles("assets", "*.gif"));
            if (imageFiles.Count > 0)
            {
                var random = new Random();
                var randomImagePath = imageFiles[random.Next(imageFiles.Count)];
                this.pictureBox1.Image = Image.FromFile(randomImagePath);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MAXIMIZE = 0xF030;
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_MAXIMIZE)
            {
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;
                int newWidth = screen.Width / 2;
                int newHeight = screen.Height / 2;
                this.Size = new Size(newWidth, newHeight);
                this.Location = new Point((screen.Width - newWidth) / 2, (screen.Height - newHeight) / 2);
                return;
            }
            base.WndProc(ref m);
        }

        private void AlarmForm_Resize(object sender, EventArgs e)
        {
            float newSize = this.ClientSize.Height / 15.0F;
            if (newSize < 12.0F) newSize = 12.0F;
            this.label1.Font = new Font(this.label1.Font.FontFamily, newSize, this.label1.Font.Style);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _autoCloseTimer.Stop();
            _autoCloseTimer.Dispose();
            bool isUserClose = !_isAutoClosing;
            FormClosedCallback?.Invoke(isUserClose);
            base.OnFormClosed(e);
        }
    }
}