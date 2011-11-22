using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel_Advisor.Errors;
using System.IO;
using Parallel_Advisor.Util;
using Parallel_Advisor.Scanning;
using System.Windows;

namespace Parallel_Advisor.Conversion
{
    public class ParallelBarrierConverter : BarrierPragmaScanner, IConverter
    {
        private int barrierIndex = 0;
        private int pragmaLine = 0;

        /// <summary>
        /// Creates a new instance of class ParallelAtomicConverter.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ParallelBarrierConverter(string file)
            : base(file)
        {
        }

        /// <summary>
        /// Scans the given source code and converts it into parallel code.
        /// </summary>
        /// <param name="outputFile">The file that contains the converted code.</param>
        public bool ScanAndConvert(string outputFile)
        {
            string[] codeLines = ReadContent(SourceFile);
            int currentIndex = 0;
            int lineCounter = codeLines.Length;

            if (codeLines[codeLines.Length - 1].StartsWith(
                "// -embedded line, required for future conversions, do not alter- "))
            {
                if (Int32.TryParse(codeLines[codeLines.Length - 1].Substring(
                    "// -embedded line, required for future conversions, do not alter- ".Length), out barrierIndex))
                {
                }
                else
                {
                    barrierIndex = 0;
                }
            }

            while (currentIndex < lineCounter)
            {
                int currentLine = currentIndex + 1;
                string line = codeLines[currentIndex];
                ErrorTypes errorType = CheckForPragmas(line);

                switch (errorType)
                {
                    case ErrorTypes.Invalid:
                        break;
                    case ErrorTypes.Valid:
                        pragmaLine = currentLine;
                        ConvertBarrier(codeLines, outputFile);
                        ScanAndConvert(outputFile);
                        return true;
                    case ErrorTypes.Missing:
                        break;
                }
                currentIndex++;
            }

            if (pragmaLine == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts a barrier pragma to a barrier statement.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="outputFile">The output file.</param>
        private void ConvertBarrier(string[] codeLines, string outputFile)
        {
            barrierIndex++;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine - 1; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine - 1].TrimEnd().Length - codeLines[pragmaLine - 1].Trim().Length;
            string indent = new string(' ', indentation);

            string code = indent + "System.Threading.Thread.MemoryBarrier();\r\n";

            StringBuilder region = new StringBuilder();
            region.Append("\r\n" + indent + "#region Parallel Section " + barrierIndex + " parallel membarrier\r\n\r\n");
            region.Append(code);
            region.Append("\r\n" + indent + "#endregion\r\n\r\n");

            writer.Write(region.ToString());

            if (codeLines[codeLines.Length - 1] ==
                "// -embedded line, required for future conversions, do not alter- " + (barrierIndex - 1))
            {
                codeLines[codeLines.Length - 1] = "// -embedded line, required for future conversions, do not alter- " + (barrierIndex);
            }

            for (int i = pragmaLine; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            if (codeLines[codeLines.Length - 1] !=
                "// -embedded line, required for future conversions, do not alter- " + barrierIndex)
            {
                writer.Write("// -embedded line, required for future conversions, do not alter- " + barrierIndex);
            }

            writer.Close();

            int lines = UtilOperations.CountLinesInString(region.ToString()) - 2;
            Main.Application.Intervals.Add(pragmaLine + 1, pragmaLine + lines);

            List<KeyValuePair<int, int>> toRemove = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> toAdd = new List<KeyValuePair<int, int>>();

            foreach (KeyValuePair<int, int> interval in Main.Application.Intervals)
            {
                if (interval.Key > pragmaLine + 1)
                {
                    toAdd.Add(new KeyValuePair<int, int>(interval.Key + 6, interval.Value + 6));
                    toRemove.Add(interval);
                }
                else
                {
                    if (interval.Key != pragmaLine + 1 && interval.Value > pragmaLine + 1)
                    {
                        toAdd.Add(new KeyValuePair<int, int>(interval.Key, interval.Value + 6));
                        toRemove.Add(interval);
                    }
                }
            }

            foreach (KeyValuePair<int, int> interval in toRemove)
            {
                Main.Application.Intervals.Remove(interval.Key);
            }

            foreach (KeyValuePair<int, int> interval in toAdd)
            {
                Main.Application.Intervals.Add(interval.Key, interval.Value);
            }

            SourceFile = outputFile;
        }
    }
}
