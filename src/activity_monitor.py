import time
from pynput import mouse, keyboard
from typing import Optional

class ActivityMonitor:
    def __init__(self) -> None:
        """
        Initializes the ActivityMonitor with separate counters for mouse and keyboard.
        """
        self.last_activity_time: float = time.time()
        self.mouse_listener: Optional[mouse.Listener] = None
        self.keyboard_listener: Optional[keyboard.Listener] = None
        
        # Add counters for activity tracking
        self.mouse_active_time: float = 0.0
        self.keyboard_active_time: float = 0.0
        self.last_mouse_time: float = time.time()
        self.last_keyboard_time: float = time.time()

    def _update_mouse_activity(self, *args) -> None:
        """
        Updates mouse activity time when mouse events occur.
        """
        current_time = time.time()
        # Only count if less than 1 second has passed (to avoid counting idle time)
        if current_time - self.last_mouse_time < 1.0:
            self.mouse_active_time += current_time - self.last_mouse_time

        self.last_mouse_time = current_time
        self._update_last_activity_time()

    def _update_keyboard_activity(self, *args) -> None:
        """
        Updates keyboard activity time when keyboard events occur.
        """
        current_time = time.time()
        # Only count if less than 1 second has passed (to avoid counting idle time)
        if current_time - self.last_keyboard_time < 1.0:
            self.keyboard_active_time += current_time - self.last_keyboard_time
        self.last_keyboard_time = current_time
        self._update_last_activity_time()

    def _update_last_activity_time(self, *args) -> None:
        """
        Updates the last activity time whenever a mouse or keyboard event is detected.
        """
        self.last_activity_time = time.time()

    def start(self) -> None:
        """
        Starts the mouse and keyboard listeners in separate threads.
        """
        self.mouse_listener = mouse.Listener(
            on_move=self._update_mouse_activity,
            on_click=self._update_mouse_activity,
            on_scroll=self._update_mouse_activity
        )
        self.keyboard_listener = keyboard.Listener(
            on_press=self._update_keyboard_activity
        )

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
        """
        return time.time() - self.last_activity_time

    def get_activity_stats(self) -> dict:
        """
        Returns the total mouse and keyboard activity times.
        """
        return {
            'mouse_active_time': round(self.mouse_active_time, 2),
            'keyboard_active_time': round(self.keyboard_active_time, 2)
        }

    def is_active(self) -> bool:
        """
        Checks if there has been any mouse or keyboard activity.
        """
        inactive_time = self.get_inactive_time()
        return inactive_time < 1.0

if __name__ == '__main__':
    print("Starting activity monitoring...")
    monitor = ActivityMonitor()
    monitor.start()
    try:
        while True:
            inactive_time = monitor.get_inactive_time()
            stats = monitor.get_activity_stats()
            print(f"Inactive: {inactive_time:.2f}s | Mouse: {stats['mouse_active_time']}s | Keyboard: {stats['keyboard_active_time']}s", end='\r')
            time.sleep(1)
    except KeyboardInterrupt:
        monitor.stop()
        print("\nMonitoring stopped.")