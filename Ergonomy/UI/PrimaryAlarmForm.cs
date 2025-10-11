// Import necessary namespaces for system functions, drawing, and Windows Forms.
using System;
using System.Drawing;
using System.Windows.Forms;

// Define the namespace for UI-related classes.
namespace Ergonomy.UI
{
    // This class defines the primary alarm window that appears first.
    // It inherits from Form, making it a standard window.
    // The 'partial' keyword indicates that the class definition is split across multiple files (this one and the .Designer.cs file).
    public partial class PrimaryAlarmForm : Form
    {
        // This event is used to notify the MainApplicationContext when the form closes.
        // The 'bool' indicates whether the user closed it (true) or it closed automatically (false).
        public event Action<bool> FormClosedCallback;
        // This timer is used to automatically close the window after a set duration.
        private System.Windows.Forms.Timer _autoCloseTimer;
        // This flag explicitly tracks if the window was closed by the timer.
        private bool _isAutoClosing = false;

        // This is the constructor. It now accepts an AppSettings object.
        public PrimaryAlarmForm(AppSettings settings)
        {
            // This method is defined in the .Designer.cs file and creates all the visual controls.
            InitializeComponent();
            // --- FIX ---
            // Make the form always stay on top of other windows.
            this.TopMost = true;

            // Set the form's starting position to be manually controlled.
            this.StartPosition = FormStartPosition.Manual;
            // Get the working area of the primary screen (the desktop area, excluding the taskbar).
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            // Set the form's location to the bottom-right corner of the screen.
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);

            // Load a random image into the picture box.
            LoadRandomImage();

            // Create a new Timer object for the auto-close functionality.
            _autoCloseTimer = new System.Windows.Forms.Timer();
            // Set the timer's interval from the settings file (converted from seconds to milliseconds).
            _autoCloseTimer.Interval = settings.PrimaryAlarmAutoCloseSeconds * 1000;
            // Define what happens when the timer's interval is reached.
            _autoCloseTimer.Tick += (sender, e) => {
                // 1. Set the flag to true to indicate this is an automatic close.
                _isAutoClosing = true;
                // 2. Close the form.
                this.Close();
            };
            // Start the auto-close timer.
            _autoCloseTimer.Start();
        }

        // This method loads a random image from the assets folder.
        private void LoadRandomImage()
        {
            // Create a list to hold all found image files.
            var imageFiles = new System.Collections.Generic.List<string>();
            // Find all .png files in the assets directory and add them to the list.
            imageFiles.AddRange(System.IO.Directory.GetFiles("assets", "*.png"));
            // Find all .gif files in the assets directory and add them to the list.
            imageFiles.AddRange(System.IO.Directory.GetFiles("assets", "*.gif"));

            // Check if any image files were found to avoid errors.
            if (imageFiles.Count > 0)
            {
                // Create a single Random instance to ensure good randomness.
                var random = new Random();
                // Select a random file path from the list of found files.
                var randomImagePath = imageFiles[random.Next(imageFiles.Count)];
                // Load the randomly selected image into the picture box.
                this.pictureBox1.Image = Image.FromFile(randomImagePath);
            }
        }

        // This method is called after the form has finished closing.
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Stop the auto-close timer to prevent it from running in the background.
            _autoCloseTimer.Stop();
            // Release the resources used by the timer.
            _autoCloseTimer.Dispose();

            // A "user close" is now defined as any close that was NOT an auto-close.
            // We invoke the callback with the opposite of our flag.
            // If timer closed it: _isAutoClosing is true, so we send 'false'.
            // If user clicked 'X': _isAutoClosing is false, so we send 'true'.
            bool isUserClose = !_isAutoClosing;
            FormClosedCallback?.Invoke(isUserClose);

            // Call the base method to complete the form closing logic.
            base.OnFormClosed(e);
        }
    }
}