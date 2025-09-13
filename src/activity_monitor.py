import time
from pynput import mouse, keyboard

class ActivityMonitor:
    def __init__(self):
        self.last_activity_time = time.time()
        self.mouse_listener = None
        self.keyboard_listener = None

    def _update_last_activity_time(self, *args):
        self.last_activity_time = time.time()

    def start(self):
        self.mouse_listener = mouse.Listener(on_move=self._update_last_activity_time, on_click=self._update_last_activity_time, on_scroll=self._update_last_activity_time)
        self.keyboard_listener = keyboard.Listener(on_press=self._update_last_activity_time)

        self.mouse_listener.start()
        self.keyboard_listener.start()

    def stop(self):
        if self.mouse_listener:
            self.mouse_listener.stop()
        if self.keyboard_listener:
            self.keyboard_listener.stop()

    def get_inactive_time(self):
        return time.time() - self.last_activity_time

if __name__ == '__main__':
    monitor = ActivityMonitor()
    monitor.start()
    try:
        while True:
            inactive_time = monitor.get_inactive_time()
            print(f"Inactive for: {inactive_time:.2f} seconds")
            time.sleep(1)
    except KeyboardInterrupt:
        monitor.stop()
        print("Monitoring stopped.")
