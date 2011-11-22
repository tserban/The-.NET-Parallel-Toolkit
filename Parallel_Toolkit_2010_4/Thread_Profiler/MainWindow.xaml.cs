using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thread_Profiler.Threads;
using Microsoft.Win32;
using Thread_Profiler.Util;

namespace Thread_Profiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory = null;

        public MainWindow()
        {
            InitializeComponent();
            currentDirectory = Environment.CurrentDirectory;
            nameLabel.Text += ProcessorInformation.Name;
            processorsLabel.Text += ProcessorInformation.PhysicalProcessors;
            coresLabel.Text += ProcessorInformation.Cores;
            logicalProcessorsLabel.Text += ProcessorInformation.LogicalProcessors;
            if (ProcessorInformation.LogicalProcessors > ProcessorInformation.Cores)
            {
                htLabel.Text += "Yes";
            }
            else
            {
                htLabel.Text += "No";
            }
        }

        /// <summary>
        /// Event handler for samplingButton_Click.
        /// </summary>
        /// <param name="sender">The control that triggered the event.</param>
        /// <param name="e">The event args.</param>
        private void samplingButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.CurrentDirectory = currentDirectory;
            ThreadProfiler profiler = new ThreadProfiler();
            int duration = 0;
            if (!Int32.TryParse(durationTextBox.Text, out duration) || duration < 1)
            {
                MessageBox.Show("Duration must be a positive integer!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                try
                {
                    ThreadInfo[] threads = profiler.SampleExecution(duration, browseTextBox.Text, argsTextBox.Text);
                    threadListView.Items.Clear();
                    foreach (ThreadInfo info in threads)
                    {
                        threadListView.Items.Add(info);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to run the given executable!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logging.Log(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Event handler for browseButton_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Executable Files|*.exe";
            if (fileDialog.ShowDialog() == true)
            {
                browseTextBox.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Event handler for MenuItem_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (threadListView.SelectedItem != null)
            {
                threadListView.Items.Remove(threadListView.SelectedItem);
            }
        }
    }
}
