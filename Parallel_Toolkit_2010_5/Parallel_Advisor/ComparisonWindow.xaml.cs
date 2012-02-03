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
using System.IO;
using System.Text.RegularExpressions;

namespace Parallel_Advisor
{
    /// <summary>
    /// Interaction logic for ComparisonWindow.xaml
    /// </summary>
    public partial class ComparisonWindow : Window
    {
        public string[] InitialContent { get; set; }
        public string[] FinalContent { get; set; }
        private int currentPragma = 0;

        /// <summary>
        /// Creates a new instance of class ComparisonWindow.
        /// </summary>
        /// <param name="initialFile">The initial file.</param>
        /// <param name="finalFile">The final file.</param>
        public ComparisonWindow(string[] initialFile, string[] finalFile)
        {
            InitializeComponent();
            InitialContent = initialFile;
            FinalContent = finalFile;
            FillTextBoxes();
            Main.Application.PragmaLines.Sort();
        }

        /// <summary>
        /// Fills the textboxes with the contents of the two files.
        /// </summary>
        private void FillTextBoxes()
        {
            initialCodeTextBox.SetValue(Paragraph.LineHeightProperty, 0.5);
            initialCodeTextBox.Document.PageWidth = 1500;
            StringBuilder initialCode = new StringBuilder();
            foreach (string line in InitialContent)
            {
                initialCode.Append(line + "\r\n");
            }
            initialCodeTextBox.AppendText(initialCode.ToString());

            foreach (int pragmaLine in Main.Application.PragmaLines)
            {
                initialCodeTextBox.Document.Blocks.ElementAt(pragmaLine).Background = Brushes.Blue;
                initialCodeTextBox.Document.Blocks.ElementAt(pragmaLine).Foreground = Brushes.White;
            }

            finalCodeTextBox.SetValue(Paragraph.LineHeightProperty, 0.5);
            finalCodeTextBox.Document.PageWidth = 1500;
            StringBuilder finalCode = new StringBuilder();
            foreach (string line in FinalContent)
            {
                finalCode.Append(line + "\r\n");
            }
            finalCodeTextBox.AppendText(finalCode.ToString());

            foreach (KeyValuePair<int, int> interval in Main.Application.Intervals)
            {
                for (int i = interval.Key - 1; i < interval.Value; i++)
                {
                    finalCodeTextBox.Document.Blocks.ElementAt(i).Background = Brushes.Blue;
                    finalCodeTextBox.Document.Blocks.ElementAt(i).Foreground = Brushes.White;
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
            double size = 14.5;
            if (Main.Application.PragmaLines.Count > 0)
            {
                if (currentPragma != 0)
                {
                    initialCodeScrollViewer.ScrollToVerticalOffset(Main.Application.PragmaLines.ElementAt(currentPragma) * size);
                    finalCodeScrollViewer.ScrollToVerticalOffset((Main.Application.Intervals.ElementAt(currentPragma).Key - 2) * size);
                }
                else
                {
                    initialCodeScrollViewer.ScrollToVerticalOffset(Main.Application.PragmaLines.ElementAt(currentPragma) * size);
                    finalCodeScrollViewer.ScrollToVerticalOffset((Main.Application.Intervals.ElementAt(currentPragma).Key - 2) * size);
                }

                if (currentPragma == Main.Application.PragmaLines.Count - 1)
                {
                    currentPragma = 0;
                }
                else
                {
                    currentPragma++;
                }
            }
        }
    }
}
