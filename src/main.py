import time
import tkinter as tk
from enum import Enum, auto
from typing import Optional
import sys
import os
from activity_monitor import ActivityMonitor
from notification_window import NotificationWindow


def get_resource_path(relative_path):
    """Get absolute path to resource for both script and frozen exe"""
    if getattr(sys, 'frozen', False):
        # Running in a bundle
        base_path = sys._MEIPASS
    else:
        # Running in normal Python environment
        base_path = os.path.dirname(os.path.dirname(__file__))
    return os.path.join(base_path, relative_path)


if getattr(sys, 'frozen', False):
    module_path = os.path.dirname(sys.executable)
else:
    module_path = os.path.dirname(os.path.dirname(__file__))
sys.path.append(module_path)


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
        self.notification_window: Optional[NotificationWindow] = None

        # --- Configuration ---
        # The duration of continuous activity or the fixed interval for notifications.
        self.notification_interval: int = 5  # 2 hours in seconds
        # self.notification_interval: int = 15 # For testing: 15 seconds
        
        # The period of inactivity after which the active time counter resets.
        self.inactivity_timeout: int = 5  # 60 seconds

        self.is_notification_auto_closed: bool = False  # New flag to track auto-close


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
        It checks the notification status and then behaves based on the current application state.
        """
        # First, check the status of the notification window.
        is_notification_open = self.notification_window is not None and self.notification_window.winfo_exists()

        # If the window is not open, run the state machine to see if we need to open one.
        if not is_notification_open:
            if self.state == AppState.MONITORING_ACTIVITY:
                self.handle_monitoring_state()
            elif self.state == AppState.WAITING_FOR_TIMER:
                self.handle_waiting_state()

        # Schedule this method to be called again after 1 second.
        self.root.after(1000, self.main_loop)
    def handle_monitoring_state(self) -> None:
        """
        Handles the logic for when the app is monitoring user activity.
        Only accumulates time when user is actively using mouse/keyboard.
        """
        current_time = time.time()
        inactive_time = self.activity_monitor.get_inactive_time()
        
        if inactive_time < self.inactivity_timeout:
            # Only add to active_time when user is actually active
            elapsed = current_time - self.last_check_time
            if self.activity_monitor.is_active():  # Check if there's actual activity
                self.active_time += elapsed
        else:
            # Reset counter if user has been inactive
            self.active_time = 0
        
        self.last_check_time = current_time
        print(f"[Monitoring] Active time: {int(self.active_time)}s / {self.notification_interval}s", end='\r')

        if self.active_time >= self.notification_interval:  # Changed > to >= for exactness
            self.show_notification()
            self.state = AppState.WAITING_FOR_TIMER
            self.notification_timer_start = time.time()
            print("\nTransitioning to WAITING_FOR_TIMER state.")

    def handle_waiting_state(self) -> None:
        """
        Handles the logic for when the app is waiting for the 2-hour timer.
        """
        elapsed = time.time() - self.notification_timer_start
        print(f"[Waiting] Next notification in: {int(self.notification_interval - elapsed)}s", end='\r')
        
        if elapsed > self.notification_interval:
            self.show_notification()
            # Reset the timer for the next interval
            self.notification_timer_start = time.time()

    def show_notification(self) -> None:
        print("\nShowing notification...")
        
        message = None
        if self.close_count >= 2:
            message = '''
                    شما پنجره ورزش کردن را 3 بار بستید، لطفا به ورزش کردن بیشتر اهمیت دهید، این پیام به مدت 
            '''
            self.close_count = 0  # Reset counter after showing warning
        
        self.notification_window = NotificationWindow(
            on_close=self.on_notification_close,
            message=message
        )
            

    def on_notification_close(self, is_user_close: bool = False) -> None:
        """
        Callback method executed when notification window is closed.
        Only increments close_count if user manually closed the window.
        """
        if is_user_close:
            self.close_count += 1
            print(f"\nUser dismissed notification. New close count: {self.close_count}")
        else:
            print("\nNotification auto-closed")

            # Ensure window is properly destroyed
        if self.notification_window:
            self.notification_window.destroy()
            self.notification_window = None

        # Reset active time and switch back to monitoring state
        self.active_time = 0
        self.state = AppState.MONITORING_ACTIVITY
        self.last_check_time = time.time()
        self.notification_window = None
        print("\nReturning to MONITORING_ACTIVITY state.")


if __name__ == "__main__":
    root = tk.Tk()
    root.withdraw()
    
    app = ErgonomyApp(root)
    app.start()
    
    root.mainloop()
    print("\nApplication has been closed.")
