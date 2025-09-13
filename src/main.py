import time
import tkinter as tk
from enum import Enum, auto
from activity_monitor import ActivityMonitor
from notification_window import NotificationWindow

class AppState(Enum):
    """
    Defines the possible states of the application's main logic.
    """
    MONITORING_ACTIVITY = auto()  # Initial state, waiting for 2 hours of activity.
    WAITING_FOR_TIMER = auto()    # Waiting for a fixed 2-hour interval to pass.

class ErgonomyApp:
    """
    The main application class that orchestrates the activity monitoring and notification display.
    """
    def __init__(self, root: tk.Tk) -> None:
        """
        Initializes the main application.

        Args:
            root (tk.Tk): The root tkinter window.
        """
        self.root: tk.Tk = root
        self.activity_monitor: ActivityMonitor = ActivityMonitor()

        # --- State Management ---
        self.state: AppState = AppState.MONITORING_ACTIVITY
        self.close_count: int = 0
        self.active_time: float = 0.0
        self.last_check_time: float = time.time()
        self.notification_timer_start: float = 0.0
        self.is_notification_open: bool = False

        # --- Configuration ---
        # The duration of continuous activity or the fixed interval for notifications.
        self.notification_interval: int = 2 * 60 * 60  # 2 hours in seconds
        # self.notification_interval: int = 15 # For testing: 15 seconds

        # The period of inactivity after which the active time counter resets.
        self.inactivity_timeout: int = 60  # 60 seconds

    def start(self) -> None:
        """
        Starts the application's main logic.
        """
        print("Starting Ergonomy App in MONITORING_ACTIVITY state...")
        self.activity_monitor.start()
        self.main_loop()

    def main_loop(self) -> None:
        """
        The main loop of the application, which is executed every second.
        It behaves differently based on the current application state.
        """
        if self.state == AppState.MONITORING_ACTIVITY:
            self.handle_monitoring_state()
        elif self.state == AppState.WAITING_FOR_TIMER:
            self.handle_waiting_state()

        # Schedule this method to be called again after 1 second.
        self.root.after(1000, self.main_loop)

    def handle_monitoring_state(self) -> None:
        """
        Handles the logic for when the app is monitoring user activity.
        """
        current_time = time.time()
        inactive_time = self.activity_monitor.get_inactive_time()

        if inactive_time < self.inactivity_timeout:
            self.active_time += current_time - self.last_check_time
        else:
            self.active_time = 0

        self.last_check_time = current_time
        print(f"[Monitoring] Active time: {int(self.active_time)}s / {self.notification_interval}s", end='\r')

        if self.active_time > self.notification_interval and not self.is_notification_open:
            self.show_notification()
            # Transition to the timer-based state
            self.state = AppState.WAITING_FOR_TIMER
            self.notification_timer_start = time.time()
            print("\nTransitioning to WAITING_FOR_TIMER state.")

    def handle_waiting_state(self) -> None:
        """
        Handles the logic for when the app is waiting for the 2-hour timer.
        """
        elapsed = time.time() - self.notification_timer_start
        print(f"[Waiting] Next notification in: {int(self.notification_interval - elapsed)}s", end='\r')

        if elapsed > self.notification_interval and not self.is_notification_open:
            self.show_notification()
            # Reset the timer for the next interval
            self.notification_timer_start = time.time()

    def show_notification(self) -> None:
        """
        Displays the notification window.
        """
        self.is_notification_open = True
        print("\nShowing notification...")

        message = None
        if self.close_count >= 3:
            message = "شما 3 بار ورزش را نادیده گرفتید، لطفا به سلامت خود بیشتر اهمیت دهید"

        notification = NotificationWindow(on_close=self.on_notification_close, message=message)

        # This is a blocking call that waits until the window is closed (by user or automatically).
        notification.mainloop()

        # After mainloop ends, the window is gone.
        self.is_notification_open = False
        print("\nNotification window closed.")


    def on_notification_close(self) -> None:
        """
        Callback method that is executed ONLY when the notification window is closed by the user.
        """
        self.close_count += 1
        print(f"User dismissed notification. New close count: {self.close_count}")


if __name__ == "__main__":
    root = tk.Tk()
    root.withdraw()

    app = ErgonomyApp(root)
    app.start()

    root.mainloop()
    print("\nApplication has been closed.")
