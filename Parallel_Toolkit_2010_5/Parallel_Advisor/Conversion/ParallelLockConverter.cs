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
    public class ParallelLockConverter : LockPragmaScanner, IConverter
    {
        private int lockIndex = 0;
        private int pragmaLine = 0;

        /// <summary>
        /// Creates a new instance of class ParallelLockConverter.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ParallelLockConverter(string file)
            : base(file)
        {
        }

        /// <summary>
        /// Scans the given source code and converts it into parallel code.
        /// </summary>
        /// <param name="outputFile">The file that contains the converted code.</param>
        public bool ScanAndConvert(string outputFile)
        {
            List<string> temp = new List<string>();
            string[] codeLines = ReadContent(SourceFile);
            int currentIndex = 0;
            int lineCounter = codeLines.Length;

            if (codeLines[codeLines.Length - 1].StartsWith(
                "// -embedded line, required for future conversions, do not alter- "))
            {
                if (Int32.TryParse(codeLines[codeLines.Length - 1].Substring(
                    "// -embedded line, required for future conversions, do not alter- ".Length), out lockIndex))
                {
                }
                else
                {
                    lockIndex = 0;
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
                        ErrorTypes returnType = FindScope(codeLines, currentIndex + 1, "{", false);
                        switch (returnType)
                        {
                            case ErrorTypes.Invalid:
                                break;
                            case ErrorTypes.Valid:
                                pragmaLine = currentLine;
                                ConvertLock(codeLines, lockObject, outputFile);
                                ScanAndConvert(outputFile);
                                return true;
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
        /// Converts a lock pragma to a lock statement.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="lockObject">The synchronization object.</param>
        /// <param name="outputFile">The output file.</param>
        private void ConvertLock(string[] codeLines, string lockObject, string outputFile)
        {
            lockIndex++;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine - 1; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine].TrimEnd().Length - codeLines[pragmaLine].Trim().Length;
            string indent = new string(' ', indentation);

            string codeBefore = null;
            string code = null;

            codeBefore = indent + "lock(" + lockObject + ")\r\n";

            if (currentScopeStartLine == currentScopeEndLine)
            {
                code += indent + "" + codeLines[currentScopeStartLine].Trim().Substring(0, currentScopeEnd + 1) + "\r\n";
            }
            else
            {
                code += indent + "" + codeLines[currentScopeStartLine].Trim() + "\r\n";

                for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                {
                    if (i == currentScopeEndLine)
                    {
                        int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                        string spacing = new string(' ', spaces);
                        code += spacing + "" + codeLines[i].Substring(spaces, currentScopeEnd + 1) + "\r\n";
                    }
                    else
                    {
                        code += "" + codeLines[i] + "\r\n";
                    }
                }
            }

            string leftover = codeLines[currentScopeEndLine].Trim().Substring(currentScopeEnd + 1);

            StringBuilder region = new StringBuilder();
            region.Append("\r\n" + indent + "#region Parallel Section " + lockIndex + " parallel lock\r\n\r\n");
            region.Append(codeBefore);
            region.Append(code);
            region.Append("\r\n" + indent + "#endregion\r\n\r\n");

            writer.Write(region.ToString());

            writer.WriteLine(indent + leftover);

            if (codeLines[codeLines.Length - 1] ==
                "// -embedded line, required for future conversions, do not alter- " + (lockIndex - 1))
            {
                codeLines[codeLines.Length - 1] = "// -embedded line, required for future conversions, do not alter- " + (lockIndex);
            }

            for (int i = currentScopeEndLine + 1; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            if (codeLines[codeLines.Length - 1] !=
                "// -embedded line, required for future conversions, do not alter- " + lockIndex)
            {
                writer.Write("// -embedded line, required for future conversions, do not alter- " + lockIndex);
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
                    toAdd.Add(new KeyValuePair<int, int>(interval.Key + lines - (currentScopeEndLine - pragmaLine) + 1, interval.Value + lines - (currentScopeEndLine - pragmaLine) + 1));
                    toRemove.Add(interval);
                }
                else
                {
                    if (interval.Key != pragmaLine + 1 && interval.Value > pragmaLine + 1)
                    {
                        toAdd.Add(new KeyValuePair<int, int>(interval.Key, interval.Value + lines - (currentScopeEndLine - pragmaLine) + 1));
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