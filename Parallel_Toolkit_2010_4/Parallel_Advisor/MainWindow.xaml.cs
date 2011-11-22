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
using Microsoft.Win32;
using Parallel_Advisor.Scanning;
using Parallel_Advisor.Errors;
using Parallel_Advisor.Conversion;
using Parallel_Advisor.Util;
using System.IO;

namespace Parallel_Advisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory = null;

        /// <summary>
        /// Creates a new instance of class MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            currentDirectory = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Event handler for browseButton_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "C# Source Files|*.cs";
            if (fileDialog.ShowDialog() == true)
            {
                browseTextBox.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Event handler for analyzeButton_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void analyzeButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.CurrentDirectory = currentDirectory;
            outputListBox.Items.Clear();
            try
            {
                List<Error> errorList = new ForPragmaScanner(browseTextBox.Text).ScanFile().ToList();
                errorList.AddRange(new WhilePragmaScanner(browseTextBox.Text).ScanFile());
                errorList.AddRange(new TasksPragmaScanner(browseTextBox.Text).ScanFile());
                errorList.AddRange(new LockPragmaScanner(browseTextBox.Text).ScanFile());
                errorList.AddRange(new AtomicPragmaScanner(browseTextBox.Text).ScanFile());
                errorList.AddRange(new BarrierPragmaScanner(browseTextBox.Text).ScanFile());
                errorList.Sort();

                if (errorList.Count == 0)
                {
                    Error noError = new Error(0, "No problems found.", WarningLevels.NonFatal);
                    noError.IconPath = "Images\\ok_sign.png";
                    outputListBox.Items.Add(noError);
                }
                else
                {
                    foreach (Error error in errorList)
                    {
                        outputListBox.Items.Add(error);
                    }
                }

                if (detailsCheckbox.IsChecked == true)
                {
                    string[] initialContent = ReadFile(browseTextBox.Text);
                    new WarningsWindow(errorList, initialContent).ShowDialog();
                }
            }
            catch(Exception ex)
            {
                Logging.Log(ex.ToString());
                MessageBox.Show("Unable to open the specified file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler for MenuItem_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (outputListBox.SelectedItem != null)
            {
                outputListBox.Items.Remove(outputListBox.SelectedItem);
            }
        }

        /// <summary>
        /// Event handler for convertButton_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void convertButton_Click(object sender, RoutedEventArgs e)
        {
            string initialFile = browseTextBox.Text;
            string finalFile = convertTextBox.Text;
            Environment.CurrentDirectory = currentDirectory;
            outputListBox.Items.Clear();
            Main.Application.PragmaLines.Clear();
            Main.Application.Intervals.Clear();
            try
            {
                List<Error> errorList = new ForPragmaScanner(initialFile).ScanFile().ToList();
                errorList.AddRange(new WhilePragmaScanner(initialFile).ScanFile());
                errorList.AddRange(new TasksPragmaScanner(initialFile).ScanFile());
                errorList.AddRange(new LockPragmaScanner(initialFile).ScanFile());
                errorList.AddRange(new AtomicPragmaScanner(initialFile).ScanFile());
                errorList.AddRange(new BarrierPragmaScanner(browseTextBox.Text).ScanFile());
                errorList.Sort();

                if (errorList.Count == 0)
                {
                    Error noError = new Error(0, "No problems found.", WarningLevels.NonFatal);
                    noError.IconPath = "Images\\ok_sign.png";
                    outputListBox.Items.Add(noError);

                    try
                    {
                        ApplyConversions(initialFile, finalFile);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log(ex.ToString());
                        MessageBox.Show("Unable to create the specified output file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                {
                    foreach (Error error in errorList)
                    {
                        outputListBox.Items.Add(error);
                    }
                    MessageBoxResult result = MessageBox.Show("The scan of the specified file yielded warnings. " +
                        "Your parallel code may not perform as expected. Are you sure you want to continue?", "Warning",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            ApplyConversions(initialFile, finalFile);
                        }
                        catch (Exception ex)
                        {
                            Logging.Log(ex.ToString());
                            MessageBox.Show("Unable to create the specified output file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log(ex.ToString());
                MessageBox.Show("Unable to open the specified file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Applies all conversions to the initial file and produces the final file.
        /// </summary>
        /// <param name="initialFile">The initial file.</param>
        /// <param name="finalFile">The final file.</param>
        private void ApplyConversions(string initialFile, string finalFile)
        {
            Parallelizer.ApplyConversions(initialFile, finalFile);

            if (comparisonCheckbox.IsChecked == true)
            {
                string[] initialContent = ReadFile(initialFile);
                MessageBox.Show("Conversion successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                string[] finalContent = ReadFile(finalFile);
                ComparisonWindow comparisonWindow = new ComparisonWindow(initialContent, finalContent);
                comparisonWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Conversion successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Event handler for browseButtonBottom_Click.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void browseButtonBottom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "C# Source Files|*.cs";
            if (fileDialog.ShowDialog() == true)
            {
                convertTextBox.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Reads a file and returns its lines.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns>The array of lines from the file.</returns>
        private string[] ReadFile(string file)
        {
            List<string> result = new List<string>();

            int lineIndex = 1;
            TextReader reader = new StreamReader(file);
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Add(lineIndex + new String(' ', 6 - lineIndex.ToString().Length) + line);
                line = reader.ReadLine();
                lineIndex++;
            }
            reader.Close();

            return result.ToArray();
        }
    }
}
