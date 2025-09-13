import time
import tkinter as tk
from activity_monitor import ActivityMonitor
from notification_window import NotificationWindow

class ErgonomyApp:
    def __init__(self, root):
        self.root = root
        self.activity_monitor = ActivityMonitor()
        self.close_count = 0
        self.active_time = 0
        self.last_check_time = time.time()
        self.is_notification_open = False

        # Configuration
        self.activity_threshold = 2 * 60 * 60  # 2 hours in seconds
        self.inactivity_timeout = 60  # seconds

    def start(self):
        self.activity_monitor.start()
        self.check_activity()

    def check_activity(self):
        current_time = time.time()
        inactive_time = self.activity_monitor.get_inactive_time()

        if inactive_time < self.inactivity_timeout:
            self.active_time += current_time - self.last_check_time
        else:
            self.active_time = 0 # Reset if user is inactive

        self.last_check_time = current_time

        if self.active_time > self.activity_threshold and not self.is_notification_open:
            self.show_notification()
            self.active_time = 0 # Reset after showing notification

        self.root.after(1000, self.check_activity) # Check every second

    def show_notification(self):
        self.is_notification_open = True
        if self.close_count >= 3:
            message = "شما 3 بار ورزش را نادیده گرفتید، لطفا به سلامت خود بیشتر اهمیت دهید"
            notification = NotificationWindow(on_close=self.on_notification_close, message=message)
        else:
            notification = NotificationWindow(on_close=self.on_notification_close)

        notification.mainloop()

    def on_notification_close(self):
        self.close_count += 1
        self.is_notification_open = False

if __name__ == "__main__":
    root = tk.Tk()
    root.withdraw()  # Hide the main window
    app = ErgonomyApp(root)
    app.start()
    root.mainloop()
