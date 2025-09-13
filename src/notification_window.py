import tkinter as tk
from PIL import Image, ImageTk
from typing import Callable, Optional, List

class NotificationWindow(tk.Toplevel):
    """
    A custom tkinter Toplevel window to display notifications.
    It can show either an animated GIF or a text message.
    """

    def __init__(self, on_close: Callable[[], None], message: Optional[str] = None) -> None:
        """
        Initializes the notification window.

        Args:
            on_close (Callable[[], None]): A callback function to be executed when the user closes the window.
            message (Optional[str], optional): A text message to display instead of the GIF. Defaults to None.
        """
        super().__init__()
        self.on_close: Callable[[], None] = on_close

        # --- Window Configuration ---
        self.title("Time for a break!")
        self.geometry("400x400")
        self.resizable(False, False)
        # Always keep the window on top of others
        self.attributes("-topmost", True)
        # Set the action for when the user clicks the window's close ('X') button
        self.protocol("WM_DELETE_WINDOW", self._handle_user_close)

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
            self.gif_label.pack(pady=10, expand=True)
            self.frame_index: int = 0
            self.animate()

        # --- Close Button ---
        self.close_button: tk.Button = tk.Button(self, text="Close", command=self._handle_user_close, state=tk.DISABLED)
        self.close_button.pack(pady=10)

        # Enable the close button after 7 seconds
        self.after(7000, self.enable_close_button)
        # Schedule the window to close automatically after 40 seconds
        self.after(40000, self.auto_close)

    def auto_close(self) -> None:
        """
        Closes the window automatically without triggering the user-close callback.
        """
        # We need to check if the window still exists before trying to destroy it,
        # as the user might have closed it manually just before this method is called.
        if self.winfo_exists():
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
        Handles the user-initiated close event from either the 'Close' button or the window's 'X' button.
        This method calls the provided on_close callback and then destroys the window.
        """
        # Check if a callback was provided and call it
        if self.on_close:
            self.on_close()
        # Destroy the tkinter window
        self.destroy()

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

    # The mainloop call below will block until the window is closed.
    # To test the message window, you would comment out the one above.
    # print("Showing notification with a message...")
    # message = "شما 3 بار ورزش را نادیده گرفتید، لطفا به سلامت خود بیشتر اهمیت دهید"
    # notification_msg = NotificationWindow(on_close=on_close_callback, message=message)

    root.mainloop()
    print("Window closed, program finished.")
