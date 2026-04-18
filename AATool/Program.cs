using System;
using System.IO;
#if WINDOWS
using System.Threading;
using System.Windows.Forms;
#endif

namespace AATool
{
    public static class Program
    {
        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e) =>
            Debug.SaveReport(e.ExceptionObject as Exception);

#if WINDOWS
        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e) =>
            Debug.SaveReport(e.Exception);
#endif

        [STAThread]
        static void Main()
        {
            string baseDirectory = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDirectory) && Directory.Exists(baseDirectory))
                Directory.SetCurrentDirectory(baseDirectory);

            //add crash reporting events
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
#if WINDOWS
            Application.ThreadException += GlobalThreadExceptionHandler;

            //start application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            using (var main = new Main())
                main.Run();
        }
    }
}
