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
    public class ParallelAtomicConverter : AtomicPragmaScanner, IConverter
    {
        private int atomicIndex = 0;
        private int pragmaLine = 0;

        /// <summary>
        /// Creates a new instance of class ParallelAtomicConverter.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ParallelAtomicConverter(string file)
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
                    "// -embedded line, required for future conversions, do not alter- ".Length), out atomicIndex))
                {
                }
                else
                {
                    atomicIndex = 0;
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
                        ErrorTypes returnType = FindAtomic(codeLines, currentIndex + 1);
                        switch (returnType)
                        {
                            case ErrorTypes.Invalid:
                                break;
                            case ErrorTypes.Valid:
                                ScopeScanner scanner = new ScopeScanner();
                                scanner.ParseString(scope);
                                ErrorTypes newReturnType = ValidateAtomic(scanner.ProgramInternalForm);
                                switch (newReturnType)
                                {
                                    case ErrorTypes.Invalid:
                                        break;
                                    case ErrorTypes.Valid:
                                        pragmaLine = currentLine;
                                        ConvertAtomic(codeLines, outputFile);
                                        ScanAndConvert(outputFile);
                                        return true;
                                }
                                break;
                            case ErrorTypes.Missing:
                                break;
                        }
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
        /// Converts an atomic pragma to an atomic statement.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="outputFile">The output file.</param>
        private void ConvertAtomic(string[] codeLines, string outputFile)
        {
            atomicIndex++;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine - 1; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine].TrimEnd().Length - codeLines[pragmaLine].Trim().Length;
            string indent = new string(' ', indentation);

            string code = null;

            switch (opType)
            {
                case OperationTypes.Addition:
                    code = indent + "System.Threading.Interlocked.Add(ref " + variable + ", " + expression + ");\r\n";
                    break;
                case OperationTypes.Subtraction:
                    code = indent + "System.Threading.Interlocked.Add(ref " + variable + ", -1 * (" + expression + "));\r\n";
                    break;
                case OperationTypes.Increment:
                    code = indent + "System.Threading.Interlocked.Increment(ref " + variable + ");\r\n";
                    break;
                case OperationTypes.Decrement:
                    code = indent + "System.Threading.Interlocked.Decrement(ref " + variable + ");\r\n";
                    break;
            }

            string leftover = codeLines[currentScopeEndLine].Trim().Substring(currentScopeEnd + 1);

            StringBuilder region = new StringBuilder();
            region.Append("\r\n" + indent + "#region Parallel Section " + atomicIndex + " parallel atomic\r\n\r\n");
            region.Append(code);
            region.Append("\r\n" + indent + "#endregion\r\n\r\n");

            writer.Write(region.ToString());

            writer.WriteLine(indent + leftover);

            if (codeLines[codeLines.Length - 1] ==
                "// -embedded line, required for future conversions, do not alter- " + (atomicIndex - 1))
            {
                codeLines[codeLines.Length - 1] = "// -embedded line, required for future conversions, do not alter- " + (atomicIndex);
            }

            for (int i = currentScopeEndLine + 1; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            if (codeLines[codeLines.Length - 1] !=
                "// -embedded line, required for future conversions, do not alter- " + atomicIndex)
            {
                writer.Write("// -embedded line, required for future conversions, do not alter- " + atomicIndex);
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