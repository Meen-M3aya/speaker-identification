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
            //TestingDTW.sampling(0);
            //TestingDTW.sampling(11);
            //TestingDTW.sampling(63);
            //TestingDTW.TestCase(1, 23);
            //TestingDTW.TestCase(1, 0);
            //TestingDTW.TestCase(2, 55);
            TestingDTW.TestCase(3, 11);


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
