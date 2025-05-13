using System;
using System.Windows.Forms;
using Recorder.Testing;

namespace Recorder
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //TestingDTW.TestCase(1);
            TestingDTW.sampling();

            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EntryWindow());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
