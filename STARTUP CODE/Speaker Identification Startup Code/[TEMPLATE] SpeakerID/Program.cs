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
            string trainingFile = @"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case1\Complete SpeakerID Dataset\TrainingList.txt";
            string testingFile = @"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case1\Complete SpeakerID Dataset\TestingList.txt";
            int testCaseNumber = 1;

            TestingDTW.TestCase(trainingFile, testingFile, testCaseNumber);

            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
