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
            // Set the form to appear in the center of the screen.
            this.StartPosition = FormStartPosition.CenterScreen;
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
            this.label1.Text = "شما 3 بار نرمش را نادیده گرفتید، این پنجره به مدت 10 ثانیه قابلیت بسته شدن ندارد";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter; // Center the text within the label.
            //
            // SecondaryAlarmForm: The main form itself.
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 102); // The size of the window's client area.
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
    }
}