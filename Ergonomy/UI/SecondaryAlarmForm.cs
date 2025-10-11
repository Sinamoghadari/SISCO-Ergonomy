// Import necessary namespaces for system functions, drawing, and Windows Forms.
using System;
using System.Drawing;
using System.Windows.Forms;

// Define the namespace for UI-related classes.
namespace Ergonomy.UI
{
    // This class defines the secondary (escalation) alarm window.
    // It appears after the user has closed the primary alarm three times.
    public partial class SecondaryAlarmForm : Form
    {
        // This timer is used to make the window unclosable for the first 10 seconds.
        private System.Windows.Forms.Timer _unclosableTimer;
        // This timer is used to automatically close the window 7 seconds after it becomes closable.
        private System.Windows.Forms.Timer _autoCloseTimer;

        // This is the constructor. It now accepts an AppSettings object.
        public SecondaryAlarmForm(AppSettings settings)
        {
            // This method creates and configures the controls on the form.
            InitializeComponent();
            // Set the form's starting position to be manually controlled.
            this.StartPosition = FormStartPosition.Manual;
            // Get the working area of the primary screen (the desktop area, excluding the taskbar).
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            // Set the form's location to the bottom-right corner of the screen.
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);
            // Initially, disable the entire control box (minimize, maximize, close buttons).
            this.ControlBox = false;

            // Create the timer that will re-enable the close button.
            _unclosableTimer = new System.Windows.Forms.Timer();
            // Set its interval from the settings file (converted from seconds to milliseconds).
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
            // Set its interval from the settings file (converted from seconds to milliseconds).
            _autoCloseTimer.Interval = settings.SecondaryAlarmAutoCloseSeconds * 1000;
            // Define what happens when this timer's interval is reached.
            _autoCloseTimer.Tick += (sender, e) => {
                // Close the form.
                this.Close();
            };
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

        // This method contains the code to create and configure all the visual elements on the form.
        private void InitializeComponent()
        {
            // Suspend layout logic.
            this.SuspendLayout();
            //
            // label1: The main warning message on the form.
            //
            this.label1 = new System.Windows.Forms.Label();
            this.label1.AutoSize = true; // The label will resize to fit its text.
            this.label1.Font = new System.Drawing.Font("B Nazanin", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.Yes; // Set text direction for Farsi.
            this.label1.Size = new System.Drawing.Size(600, 84);
            this.label1.TabIndex = 0;
            this.label1.Text = "شما نرمش را نادیده گرفتید،\nاین پنجره به مدت 10 ثانیه بسته نگه داشته می شود";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter; // Center the text within the label.
            //
            // pictureBox1: The control that displays the animated GIF.
            //
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox1.Location = new System.Drawing.Point(12, 96);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(276, 132);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage; // The image will stretch to fit the box.
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
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
                this.pictureBox1.Image = System.Drawing.Image.FromFile(randomImagePath);
            }
            //
            // SecondaryAlarmForm: The main form itself.
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 240); // The size of the window's client area.
            this.Controls.Add(this.pictureBox1); // Add the picture box to the form.
            this.Controls.Add(this.label1); // Add the label to the form.
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog; // A fixed dialog border.
            this.MaximizeBox = false; // Disable the maximize button.
            this.MinimizeBox = false; // Disable the minimize button.
            this.Name = "SecondaryAlarmForm";
            this.Text = "هشدار"; // The text in the window's title bar.
            // Resume layout logic.
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Declaration of the label control.
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}