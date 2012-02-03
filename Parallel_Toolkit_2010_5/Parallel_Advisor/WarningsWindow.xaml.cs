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
using System.Windows.Shapes;
using Parallel_Advisor.Errors;

namespace Parallel_Advisor
{
    /// <summary>
    /// Interaction logic for WarningsWindow.xaml
    /// </summary>
    public partial class WarningsWindow : Window
    {
        public List<Error> ErrorList { get; set; }
        public string[] InitialContent { get; set; }
        private int currentError = 0;

        /// <summary>
        /// Creates a new instance of class WarningsWindow.
        /// </summary>
        /// <param name="errorList">The list of errors to display.</param>
        public WarningsWindow(List<Error> errorList, string[] initialContent)
        {
            InitializeComponent();
            ErrorList = errorList;
            InitialContent = initialContent;
            ApplyColours();
        }

        /// <summary>
        /// Applies colours to the lines with errors / warnings.
        /// </summary>
        private void ApplyColours()
        {
            initialCodeTextBox.SetValue(Paragraph.LineHeightProperty, 0.5);
            initialCodeTextBox.Document.PageWidth = 1500;
            //initialCodeTextBox.Document.FontFamily 
            StringBuilder initialCode = new StringBuilder();
            foreach (string line in InitialContent)
            {
                initialCode.Append(line + "\r\n");
            }
            initialCodeTextBox.AppendText(initialCode.ToString());

            foreach (Error error in ErrorList)
            {
                if (error.Level == WarningLevels.Fatal)
                {
                    initialCodeTextBox.Document.Blocks.ElementAt(error.Line - 1).Background = Brushes.Red;
                }
                else
                {
                    initialCodeTextBox.Document.Blocks.ElementAt(error.Line - 1).Background = Brushes.Yellow;
                }
            }
        }

        /// <summary>
        /// Event handler for nextButton_Click.
        /// </summary>
        /// <param name="sender">The control that triggered the event.</param>
        /// <param name="e">The event args.</param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (ErrorList.Count > 0)
            {
                Error error = ErrorList.ElementAt(currentError);
                initialCodeScrollViewer.ScrollToVerticalOffset(15 * error.Line - 65);
                errorTextBlock.Text = "Line " + error.Line + " : " + error.Cause;

                if (currentError == ErrorList.Count - 1)
                {
                    currentError = 0;
                }
                else
                {
                    currentError++;
                }
            }
        }
    }
}
