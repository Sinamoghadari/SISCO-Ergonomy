// The main namespace for the application.
namespace Ergonomy
{
    // This is the main class for the application.
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application. This is the first method that runs.
        /// </summary>
        [STAThread] // This attribute indicates that the application's UI model is a single-threaded apartment, which is required for Windows Forms.
        static void Main()
        {
            // This method initializes default application settings like high DPI support.
            ApplicationConfiguration.Initialize();
            // This starts the application message loop.
            // Instead of running a standard Form, we run our custom MainApplicationContext.
            // This allows the application to run without a visible main window, managed by our context class.
            Application.Run(new MainApplicationContext());
        }
    }
}