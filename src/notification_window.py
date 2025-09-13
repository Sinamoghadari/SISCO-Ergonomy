import tkinter as tk
from PIL import Image, ImageTk
from typing import Callable, Optional, List
import sys
import os

class NotificationWindow(tk.Toplevel):
    """
    A custom tkinter Toplevel window to display notifications.
    It can show either an animated GIF or a text message.
    """
    # Class-level constants for timing configuration
    close_after_3_attempts_seconds: int = 10  # Time window stays open after 3 dismissals
    auto_close_seconds: int = 5  # Time before auto-closing any notification

    def __init__(
            self, on_close: Callable[[], None],
            message: Optional[str] = None ,
            ) -> None:
        
        """
        Initializes the notification window.

        Args:
            on_close (Callable[[], None]): A callback function to be executed when the user closes the window.
            message (Optional[str], optional): A text message to display instead of the GIF. Defaults to None.
        """
        super().__init__()
        if message:
            label = tk.Label(self, text=message, font=("Arial", 12), wraplength=380, justify="center")
            label.pack(pady=20, padx=10, expand=True)
        else:
            # Get correct path whether running as script or frozen exe
            if getattr(sys, 'frozen', False):
                # Running as compiled exe
                base_path = sys._MEIPASS
            else:
                # Running as script
                base_path = os.path.abspath(os.path.dirname(os.path.dirname(__file__)))
            
            self.gif_path = os.path.join(base_path, 'assets', 'exercise.gif')
            self.gif = Image.open(self.gif_path)

        self.on_close: Callable[[], None] = on_close

        # --- Window Configuration ---
        self.title("زمان نرمش فرا رسیده است!")
        self.geometry("400x400")
        self.resizable(False, False)
        # Always keep the window on top of others
        self.attributes("-topmost", True)

        if message:
            self.overrideredirect(True)  # Removes window decorations including close button
        else:
            self.protocol("WM_DELETE_WINDOW", self._handle_user_close)

        # Position window at bottom right of screen
        self._position_bottom_right()

        # Enable the close button after 7 seconds
        self.after(10000, self.enable_close_button)

        # Schedule the window to close automatically using the new constant
        self.after(self.auto_close_seconds * 1000, self.auto_close)

        # --- Content ---
        if message:
            # Display a text message if one is provided
            label = tk.Label(self, text=message, font=("Arial", 12), wraplength=380, justify="center")
            label.pack(pady=20, padx=10, expand=True)
        else:
            # Otherwise, load and display the exercise GIF
            self.gif_path: str = "assets/exercise.gif"
            self.gif: Image.Image = Image.open(self.gif_path)
            self.frames: List[ImageTk.PhotoImage] = []
            for i in range(self.gif.n_frames):
                self.gif.seek(i)
                # Create a PhotoImage for each frame of the GIF
                self.frames.append(ImageTk.PhotoImage(self.gif.copy()))

            self.gif_label: tk.Label = tk.Label(self)
            self.gif_label.pack(pady=15, expand=True)
            self.frame_index: int = 0
            self.animate()

        # --- Close Button ---
        self.close_button: tk.Button = tk.Button(self, text="بستن", command=self._handle_user_close, state=tk.DISABLED)
        self.close_button.pack(pady=10)

        # Enable the close button after 7 seconds
        self.after(7000, self.enable_close_button)
        # Schedule the window to close automatically after 40 seconds
        self.after(40000, self.auto_close)

    def auto_close(self) -> None:
        """
        Handles automatic window closing.
        """
        if self.winfo_exists():
            if self.on_close:
                self.on_close(is_user_close=False)  # Add flag to indicate auto-close
            self.destroy()

    def animate(self) -> None:
        """
        Cycles through the GIF frames to create the animation effect.
        """
        # Update the label with the next frame
        self.gif_label.config(image=self.frames[self.frame_index])
        # Move to the next frame, looping back to the start
        self.frame_index = (self.frame_index + 1) % len(self.frames)
        # Schedule the next frame update
        self.after(100, self.animate)

    def enable_close_button(self) -> None:
        """
        Enables the 'Close' button, allowing the user to dismiss the notification.
        """
        self.close_button.config(state=tk.NORMAL)

    def _handle_user_close(self) -> None:
        """
        Handles when user explicitly closes the window.
        """
        if self.on_close:
            self.on_close(is_user_close=True)  # Add flag to indicate user closed it
        self.destroy()

    def _position_bottom_right(self) -> None:
        """
        Positions the window at the bottom right of the screen.
        """
        # Get screen width and height
        screen_width = self.winfo_screenwidth()
        screen_height = self.winfo_screenheight()
        
        # Get window size
        window_width = 400
        window_height = 400
        
        # Calculate position
        x_position = screen_width - window_width - 20  # 20px padding from right
        y_position = screen_height - window_height - 40  # 40px padding from bottom
        
        # Set window position
        self.geometry(f"+{x_position}+{y_position}")


if __name__ == '__main__':
    # This block demonstrates how to use the NotificationWindow.
    # It will only run when the script is executed directly.
    def on_close_callback() -> None:
        print("Notification was closed by the user.")

    # A hidden root window is required for Toplevel windows to work
    root = tk.Tk()
    root.withdraw()

    print("Showing notification with GIF...")
    notification_gif = NotificationWindow(on_close=on_close_callback)

    root.mainloop()
    print("Window closed, program finished.")
