import time
from pynput import mouse, keyboard
from typing import Optional

class ActivityMonitor:
    """
    Monitors user's keyboard and mouse activity to track idle time.
    """
    def __init__(self) -> None:
        """
        Initializes the ActivityMonitor.
        """
        self.last_activity_time: float = time.time()
        self.mouse_listener: Optional[mouse.Listener] = None
        self.keyboard_listener: Optional[keyboard.Listener] = None

    def _update_last_activity_time(self, *args) -> None:
        """
        Callback function to update the last activity time whenever a mouse or keyboard event is detected.
        """
        self.last_activity_time = time.time()

    def start(self) -> None:
        """
        Starts the mouse and keyboard listeners in separate threads.
        """
        self.mouse_listener = mouse.Listener(
            on_move=self._update_last_activity_time,
            on_click=self._update_last_activity_time,
            on_scroll=self._update_last_activity_time
        )
        self.keyboard_listener = keyboard.Listener(on_press=self._update_last_activity_time)

        self.mouse_listener.start()
        self.keyboard_listener.start()

    def stop(self) -> None:
        """
        Stops the mouse and keyboard listeners.
        """
        if self.mouse_listener:
            self.mouse_listener.stop()
        if self.keyboard_listener:
            self.keyboard_listener.stop()

    def get_inactive_time(self) -> float:
        """
        Calculates the time in seconds since the last detected activity.

        Returns:
            float: The number of seconds of inactivity.
        """
        return time.time() - self.last_activity_time

if __name__ == '__main__':
    # Example usage of the ActivityMonitor class.
    # This block will only execute when the script is run directly.
    print("Starting activity monitoring...")
    monitor = ActivityMonitor()
    monitor.start()
    try:
        while True:
            inactive_time = monitor.get_inactive_time()
            print(f"User has been inactive for: {inactive_time:.2f} seconds", end='\r')
            time.sleep(1)
    except KeyboardInterrupt:
        monitor.stop()
        print("\nMonitoring stopped.")
