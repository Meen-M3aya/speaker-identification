using System;
using System.Windows.Forms;

namespace Recorder
{
    public partial class EntryWindow : Form
    {
        public EntryWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnrollmentWindow enrollmentWindow = new EnrollmentWindow();
            enrollmentWindow.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainForm identification = new MainForm();
            identification.ShowDialog();
        }
    }
}
