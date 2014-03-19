using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClzCopy
{
    public partial class Form1 : Form
    {
        List<String> copyList = new List<String>();
        Color origColor;

        public Form1()
        {
            InitializeComponent();
            origColor = textBox1.BackColor;
            textBox1.Text = Properties.Settings.Default.LastPath;
        }

        public void UpdateWatcher()
        {
            try
            {
                watcher.Path = textBox1.Text;

                /* Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. */
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                // watch for Crestron clz files
                watcher.Filter = "*.clz";

                // Begin watching
                watcher.EnableRaisingEvents = true;

                textBox1.BackColor = Color.LightGreen;
            }
            catch (ArgumentException)
            {
                textBox1.BackColor = Color.Red;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateWatcher();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastPath = textBox1.Text;
            Properties.Settings.Default.Save();
            UpdateWatcher();
        }

        private void ClzChanged(string path)
        {
            // ignore our own copies
            if (Path.GetDirectoryName(path) == Path.GetDirectoryName(textBox1.Text + "\\"))
            {
                //Log("skipping copy to root");
                return;
            }

            lock (copyList)
            {
                if (!copyList.Contains(path))
                {
                    copyList.Add(path);
                }

                timer1.Stop();
                timer1.Start();
            }
        }
  
        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ClzChanged(e.FullPath);
        }

        private void Log(string s)
        {
            Console.WriteLine(s);
            textBox2.AppendText(s + "\r\n");
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            ClzChanged(e.FullPath);
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Log("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Log("File: " + e.OldFullPath + " -> " + e.FullPath + " " + e.ChangeType);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            lock (copyList)
            {
                while (copyList.Count > 0)
                {
                    string sourceFilePath = copyList[0];
                    copyList.RemoveAt(0);

                    string destFilePath = Path.Combine(textBox1.Text, Path.GetFileName(sourceFilePath));

                    Log("Copying CLZ file [" + sourceFilePath + "] to USP folder from [" + destFilePath + "]");
                    try
                    {
                        File.Copy(sourceFilePath, destFilePath, true);
                    }
                    catch(Exception ex)
                    {
                        Log("Copy failed! " + ex.ToString());
                    }
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                folderBrowserDialog1.SelectedPath = textBox1.Text;
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Log("Changing path to " + folderBrowserDialog1.SelectedPath);
                    textBox1.Text = folderBrowserDialog1.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
    }
}
