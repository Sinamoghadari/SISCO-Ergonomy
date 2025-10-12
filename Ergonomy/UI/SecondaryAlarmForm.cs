// Import necessary namespaces for system functions, drawing, and Windows Forms.
using System;
using System.Drawing;
using System.Windows.Forms;

// Define the namespace for UI-related classes.
namespace Ergonomy.UI
{
    // This class defines the secondary (escalation) alarm window.
    // It is a partial class, with its UI controls defined in the .Designer.cs file.
    public partial class SecondaryAlarmForm : Form
    {
        // This timer is used to make the window unclosable for the first 10 seconds.
        private System.Windows.Forms.Timer _unclosableTimer;
        // This timer is used to automatically close the window after it becomes closable.
        private System.Windows.Forms.Timer _autoCloseTimer;

        // This is the constructor. It now accepts an AppSettings object.
        public SecondaryAlarmForm(AppSettings settings)
        {
            // This method creates and configures the controls on the form.
            InitializeComponent();
            // --- FIX ---
            // Make the form always stay on top of other windows.
            this.TopMost = true;

            // Set the form's starting position to be manually controlled.
            this.StartPosition = FormStartPosition.Manual;
            // Get the working area of the primary screen.
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            // Set the form's location to the bottom-right corner of the screen.
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);

            // Load a random image into the picture box.
            LoadRandomImage();

            // Initially, disable the entire control box (minimize, maximize, close buttons).
            this.ControlBox = false;

            // Create the timer that will re-enable the close button.
            _unclosableTimer = new System.Windows.Forms.Timer();
            // Set its interval from the settings file.
            _unclosableTimer.Interval = settings.SecondaryAlarmUnclosableSeconds * 1000;
            // Define what happens when the timer's interval is reached.
            _unclosableTimer.Tick += (sender, e) => {
                // Re-enable the control box, making the 'X' button visible and functional.
                this.ControlBox = true;
                // Stop this timer so it doesn't run again.
                _unclosableTimer.Stop();
                // Start the second timer, which will auto-close the window after another set duration.
                _autoCloseTimer.Start();
            };
            // Start the unclosable timer.
            _unclosableTimer.Start();

            // Create the timer that will automatically close the window.
            _autoCloseTimer = new System.Windows.Forms.Timer();
            // Set its interval from the settings file.
            _autoCloseTimer.Interval = settings.SecondaryAlarmAutoCloseSeconds * 1000;
            // Define what happens when this timer's interval is reached.
            _autoCloseTimer.Tick += (sender, e) => {
                // Close the form.
                this.Close();
            };
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

        // This method overrides the default window procedure to intercept system messages.
        protected override void WndProc(ref Message m)
        {
            // Define constants for the Windows messages we need to intercept.
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MAXIMIZE = 0xF030;

            // Check if the message is a system command and if the command is to maximize.
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_MAXIMIZE)
            {
                // If it is, we handle it ourselves and do not pass it to the base class.

                // Get the primary screen's dimensions.
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;

                // Calculate the new size (50% of the screen's width and height).
                int newWidth = screen.Width / 2;
                int newHeight = screen.Height / 2;

                // Calculate the new location to center the window.
                int newX = (screen.Width - newWidth) / 2;
                int newY = (screen.Height - newHeight) / 2;

                // Apply the new size and location.
                this.Size = new Size(newWidth, newHeight);
                this.Location = new Point(newX, newY);

                // Return here to prevent the default maximization.
                return;
            }

            // For all other messages, pass them to the base WndProc to be handled normally.
            base.WndProc(ref m);
        }

        // This method is called after the form has closed (for any reason).
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Stop both timers to be safe.
            _unclosableTimer.Stop();
            _autoCloseTimer.Stop();
            // Release the resources used by both timers.
            _unclosableTimer.Dispose();
            _autoCloseTimer.Dispose();
            // Call the base method to complete the closing process.
            base.OnFormClosed(e);
        }
    }
}