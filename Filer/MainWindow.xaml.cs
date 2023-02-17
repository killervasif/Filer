using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Filer
{
    public partial class MainWindow : Window
    {
        private Thread thread;
        public string FromPath { get; set; }
        public string ToPath { get; set; }
        private bool isStopped = false;

        public MainWindow()
        {
            InitializeComponent();
            thread = new Thread(CopyMethod);
            DataContext = this;
        }

        private void CopyMethod()
        {
            Dispatcher.Invoke(() => Progress.Value = 0);

            using (FileStream fsRead = new FileStream(FromPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            {
                using (FileStream fsWrite = new FileStream(ToPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    var fileSize = fsRead.Length;
                    byte[] buffer = new byte[70];
                    var copiedBytes = 0L;

                    do
                    {
                        var bytesToCopy = Math.Min(buffer.Length, (int)(fileSize - copiedBytes));

                        fsRead.Read(buffer, 0, bytesToCopy);
                        fsWrite.Write(buffer, 0, bytesToCopy);

                        copiedBytes += bytesToCopy;
                        var progress = (int)(copiedBytes * 100 / fileSize);
                        Dispatcher.Invoke(() => Progress.Value = progress);

                        Thread.Sleep(500);
                    } while (copiedBytes < fileSize);

                }
            }
            Dispatcher.Invoke(() => btnCopy.IsEnabled = true);
        }

        private void btnFrom_Click(object sender, RoutedEventArgs e)
        {
            FileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files |*.txt";

            var result = dialog.ShowDialog();

            if (result == true)
            {                
                txtFrom.Text = dialog.FileName;
                return;
            }
        }

        private void btnTo_Click(object sender, RoutedEventArgs e)
        {
            FileDialog dialog = new SaveFileDialog();

            var result = dialog.ShowDialog();

            if (result == true)
            {
                txtTo.Text = dialog.FileName;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FromPath) || string.IsNullOrWhiteSpace(ToPath))
            {
                MessageBox.Show("Please enter all information");
                return;
            }

            if (!File.Exists(FromPath))
            {
                MessageBox.Show("Entered wrong file or folder path");
                return;
            }

            if (!ToPath.Contains(".txt"))
                ToPath += ".txt";


            btnCopy.IsEnabled = false;
            thread.Start();
        }

        private void btnSuspend_Click(object sender, RoutedEventArgs e)
        {
            if (thread.ThreadState == ThreadState.Unstarted)
                return;

            isStopped = !isStopped;

            if (isStopped)
            {
                btnSuspend.Content = "Continue";

                try
                {
                    thread.Suspend();
                }
                catch (Exception)
                {

                }

            }
            else
            {
                btnSuspend.Content = "Suspend";

                try
                {
                    thread.Resume();
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            if (thread.ThreadState == ThreadState.Unstarted)
                return;

            thread.Abort();

            if (File.Exists(ToPath))
                File.Delete(ToPath);

            btnCopy.IsEnabled = true;
            Progress.Value = 0;
        }
    }
}


