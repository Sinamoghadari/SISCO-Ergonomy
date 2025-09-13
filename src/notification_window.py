import tkinter as tk
from PIL import Image, ImageTk

class NotificationWindow(tk.Toplevel):
    def __init__(self, on_close, message=None):
        super().__init__()
        self.on_close = on_close
        self.title("Time for a break!")
        self.geometry("400x400")
        self.resizable(False, False)

        self.protocol("WM_DELETE_WINDOW", self._on_close_button)

        if message:
            label = tk.Label(self, text=message, font=("Arial", 12), wraplength=380)
            label.pack(pady=20)
        else:
            self.gif_path = "assets/exercise.gif"
            self.gif = Image.open(self.gif_path)
            self.frames = []
            for i in range(self.gif.n_frames):
                self.gif.seek(i)
                self.frames.append(ImageTk.PhotoImage(self.gif.copy()))

            self.gif_label = tk.Label(self)
            self.gif_label.pack(pady=10)
            self.frame_index = 0
            self.animate()

        self.close_button = tk.Button(self, text="Close", command=self._on_close_button, state=tk.DISABLED)
        self.close_button.pack(pady=10)

        self.after(7000, self.enable_close_button)

    def animate(self):
        self.gif_label.config(image=self.frames[self.frame_index])
        self.frame_index = (self.frame_index + 1) % len(self.frames)
        self.after(100, self.animate)

    def enable_close_button(self):
        self.close_button.config(state=tk.NORMAL)

    def _on_close_button(self):
        if self.on_close:
            self.on_close()
        self.destroy()

if __name__ == '__main__':
    def on_close_callback():
        print("Notification closed by user.")

    root = tk.Tk()
    root.withdraw()  # Hide the main window

    # Test with GIF
    notification = NotificationWindow(on_close=on_close_callback)

    # Test with message
    # notification = NotificationWindow(on_close=on_close_callback, message="شما 3 بار ورزش را نادیده گرفتید، لطفا به سلامت خود بیشتر اهمیت دهید")

    root.mainloop()
