using System;
using System.Windows.Forms;
using System.Threading;

namespace ClzCopy
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutex_id = "ClzCopy";
            using (Mutex mutex = new Mutex(false, mutex_id))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("ClzCopy is already running!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
