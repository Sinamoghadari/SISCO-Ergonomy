// Import necessary namespaces for system functions, diagnostics, and interoperability services.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Define the namespace for hook-related classes.
namespace Ergonomy.Hooks
{
    // This class sets up low-level, system-wide hooks to monitor keyboard and mouse input.
    // This is necessary to detect activity even when our application is not the active window.
    // It implements IDisposable to ensure hooks are properly removed.
    public class GlobalInputHook : IDisposable
    {
        // These are constant values from the Windows API that define the type of hook.
        // WH_KEYBOARD_LL is for a low-level keyboard hook.
        private const int WH_KEYBOARD_LL = 13;
        // WH_MOUSE_LL is for a low-level mouse hook.
        private const int WH_MOUSE_LL = 14;

        // These are constant values from the Windows API that identify specific keyboard and mouse messages.
        private const int WM_KEYDOWN = 0x0100;       // A key was pressed.
        private const int WM_SYSKEYDOWN = 0x0104;    // A system key (like Alt) was pressed.
        private const int WM_MOUSEMOVE = 0x0200;     // The mouse was moved.
        private const int WM_LBUTTONDOWN = 0x0201;   // The left mouse button was pressed.
        private const int WM_RBUTTONDOWN = 0x0204;   // The right mouse button was pressed.
        private const int WM_MBUTTONDOWN = 0x0207;   // The middle mouse button was pressed.
        private const int WM_MOUSEWHEEL = 0x020A;    // The mouse wheel was scrolled.

        // This defines the signature of the callback function that Windows will call when a hook event occurs.
        private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        // These hold references to our callback methods. This is important to prevent them from being garbage collected.
        private HookCallback _keyboardCallback;
        private HookCallback _mouseCallback;
        // These will store the "handles" (unique identifiers) for our installed hooks.
        private IntPtr _keyboardHookHandle = IntPtr.Zero;
        private IntPtr _mouseHookHandle = IntPtr.Zero;

        // This event is fired whenever any keyboard activity is detected.
        public event EventHandler KeyboardActivity;
        // This event is fired whenever any mouse activity is detected.
        public event EventHandler MouseActivity;

        // This is the constructor. It runs when a new GlobalInputHook is created.
        public GlobalInputHook()
        {
            // Assign our C# methods to the delegate variables.
            _keyboardCallback = KeyboardHookCallback;
            _mouseCallback = MouseHookCallback;
        }

        // This method installs the hooks.
        public void Start()
        {
            _keyboardHookHandle = SetHook(WH_KEYBOARD_LL, _keyboardCallback);
            _mouseHookHandle = SetHook(WH_MOUSE_LL, _mouseCallback);
        }

        // This method uninstalls the hooks.
        public void Stop()
        {
            UnhookWindowsHookEx(_keyboardHookHandle);
            UnhookWindowsHookEx(_mouseHookHandle);
        }

        // A helper method to install a hook.
        private IntPtr SetHook(int hookId, HookCallback callback)
        {
            // We need the handle of the current process module to install a global hook.
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // Call the Windows API function to set the hook and return its handle.
                return SetWindowsHookEx(hookId, callback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // This is the actual callback method for the keyboard hook. Windows calls this method.
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // nCode >= 0 means we should process the message.
            // We check if the message is a key down event.
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                // Fire our public KeyboardActivity event.
                // The '?' ensures we don't try to invoke it if nothing is subscribed.
                KeyboardActivity?.Invoke(this, EventArgs.Empty);
            }
            // IMPORTANT: Always call CallNextHookEx to pass the message to the next hook in the chain.
            // If you don't do this, other applications that use hooks will not receive the event.
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        // This is the actual callback method for the mouse hook. Windows calls this method.
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // nCode >= 0 means we should process the message.
            if (nCode >= 0)
            {
                // We check if the message type is one of the mouse activities we care about.
                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                    case WM_RBUTTONDOWN:
                    case WM_MBUTTONDOWN:
                    case WM_MOUSEMOVE:
                    case WM_MOUSEWHEEL:
                        // If it is, fire our public MouseActivity event.
                        MouseActivity?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
            // IMPORTANT: Pass the message to the next hook in the chain.
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        // This method is part of the IDisposable interface. It's called to clean up resources.
        public void Dispose()
        {
            // When the object is disposed, stop and uninstall the hooks.
            Stop();
        }

        // This region contains the P/Invoke (Platform Invoke) declarations.
        // These allow our C# code to call functions directly from native Windows DLLs like user32.dll.
        #region PInvoke
        // Declares the SetWindowsHookEx function from user32.dll.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookCallback lpfn, IntPtr hMod, uint dwThreadId);

        // Declares the UnhookWindowsHookEx function from user32.dll.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // Declares the CallNextHookEx function from user32.dll.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        // Declares the GetModuleHandle function from kernel32.dll.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}