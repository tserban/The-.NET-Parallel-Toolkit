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
    public class ParallelTasksConverter : TasksPragmaScanner, IConverter
    {
        private int tasksIndex = 0;
        private int pragmaLine = 0;
        private int currentTask = 0;

        /// <summary>
        /// Creates a new instance of class ParallelTasksConverter.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ParallelTasksConverter(string file)
            : base(file)
        {
        }

        /// Scans the given source code and converts it into parallel code.
        /// </summary>
        /// <param name="outputFile">The file that contains the converted code.</param>
        public bool ScanAndConvert(string outputFile)
        {
        scan:
            string[] codeLines = ReadContent(SourceFile);
            int lineCounter = codeLines.Length;
            int currentIndex = 0;

            if (codeLines[codeLines.Length - 1].StartsWith(
                "// -embedded line, required for future conversions, do not alter- "))
            {
                if (Int32.TryParse(codeLines[codeLines.Length - 1].Substring(
                    "// -embedded line, required for future conversions, do not alter- ".Length), out tasksIndex))
                {
                }
                else
                {
                    tasksIndex = 0;
                }
            }

            while (currentIndex < lineCounter)
            {
                int currentLine = currentIndex + 1;
                string line = codeLines[currentIndex];
                ErrorTypes errorType = CheckForPragmas(currentIndex, codeLines);

                switch (errorType)
                {
                    case ErrorTypes.Invalid:
                        break;
                    case ErrorTypes.Valid:
                        validParallelTasks.Clear();
                        pragmaLine = currentLine;
                        ValidateTasks(currentIndex, codeLines);

                        if (validParallelTasks.Count > 0)
                        {
                            int validTaskLine = validParallelTasks.First();
                            ErrorTypes returnType = FindScope(codeLines, validTaskLine + 1, "{", false);
                            switch (returnType)
                            {
                                case ErrorTypes.Invalid:
                                    break;
                                case ErrorTypes.Valid:
                                    pragmaLine = validTaskLine;
                                    ScopeScanner scanner = new ScopeScanner();
                                    scanner.ParseString(scope);
                                    InspectScope(scanner.ProgramInternalForm, validTaskLine + 1);
                                    ConvertTask(codeLines, outputFile);
                                    SourceFile = outputFile;
                                    goto scan;
                                case ErrorTypes.Missing:
                                    break;
                            }
                        }
                        else
                        {
                            ConvertTasks(codeLines, outputFile);
                            currentTask = 0;
                            SourceFile = outputFile;
                            isPooled = false;
                            noSyncOn = false;
                            goto scan;
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
        /// Sets up the team of threads and then joins them at the end of the parallel tasks region.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="outputFile">The output file.</param>
        private void ConvertTasks(string[] codeLines, string outputFile)
        {
            tasksIndex++;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine - 1; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine - 1].TrimEnd().Length - codeLines[pragmaLine - 1].Trim().Length;
            string indent = new string(' ', indentation);

            string codeBefore = null;
            string code = null;
            string codeAfter = null;
            
            for (int i = pragmaLine; i < endTasksLine; i++)
            {
                code += codeLines[i] + "\r\n";
            }

            codeBefore =
                    indent + "int TASKCOUNT" + tasksIndex + " = " + currentTask + ";\r\n" +
                    indent + "System.Threading.ParameterizedThreadStart[] TASKLIST" + tasksIndex + " = new System.Threading.ParameterizedThreadStart[TASKCOUNT" + tasksIndex + "];\r\n";

            codeAfter +=
                    indent + "Toolkit.ParallelTasks(" + noSyncOn.ToString().ToLower() + ", TASKLIST" + tasksIndex;

            if (isPooled)
            {
                codeBefore += "\r\n";
                codeAfter += ");\r\n\r\n";
            }
            else
            {
                codeBefore += indent + "bool[] SYNCMASK" + tasksIndex + " = new bool[TASKCOUNT" + tasksIndex + "];\r\n\r\n";
                codeAfter += ", " + "SYNCMASK" + tasksIndex + ");\r\n\r\n";
            }

            StringBuilder region = new StringBuilder();
            region.Append("\r\n" + indent + "#region Parallel Section " + tasksIndex + " parallel tasks\r\n\r\n");
            region.Append(codeBefore);
            region.Append(code);
            region.Append(codeAfter);
            region.Append("\r\n" + indent + "#endregion\r\n\r\n");

            writer.Write(region.ToString());

            if (codeLines[codeLines.Length - 1] ==
                "// -embedded line, required for future conversions, do not alter- " + (tasksIndex - 1))
            {
                codeLines[codeLines.Length - 1] = "// -embedded line, required for future conversions, do not alter- " + (tasksIndex);
            }

            for (int i = endTasksLine + 1; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            if (codeLines[codeLines.Length - 1] !=
                "// -embedded line, required for future conversions, do not alter- " + tasksIndex)
            {
                writer.Write("// -embedded line, required for future conversions, do not alter- " + tasksIndex);
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
                    toAdd.Add(new KeyValuePair<int, int>(interval.Key + lines - (endTasksLine - pragmaLine) + 4, interval.Value + lines - (endTasksLine - pragmaLine) + 4));
                    toRemove.Add(interval);
                }
                else
                {
                    if (interval.Key != pragmaLine + 1 && interval.Value > pragmaLine + 1)
                    {
                        toAdd.Add(new KeyValuePair<int, int>(interval.Key, interval.Value + lines - (currentScopeEndLine - pragmaLine) + 4));
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
        }

        /// <summary>
        /// Converts a task scope to a parallel task.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="outputFile">The output file.</param>
        private void ConvertTask(string[] codeLines, string outputFile)
        {
            int tempIndex = tasksIndex + 1;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine].TrimEnd().Length - codeLines[pragmaLine].Trim().Length;
            string indent = new string(' ', indentation);

            string codeBefore = null;
            string codeAfter = null;
            string code = null;

            codeBefore =
                indent + "TASKLIST" + tempIndex + "[" + currentTask + "] = \r\n" +
                indent + "    PARAM" + tempIndex + " =>\r\n";

            if (currentScopeStartLine == currentScopeEndLine)
            {
                code += indent + "    " + codeLines[currentScopeStartLine].Trim().Substring(0, currentScopeEnd + 1) + ";\r\n";
            }
            else
            {
                code += indent + "    " + codeLines[currentScopeStartLine].Trim() + "\r\n";

                for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                {
                    if (i == currentScopeEndLine)
                    {
                        int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                        string spacing = new string(' ', spaces);
                        code += spacing + "    " + codeLines[i].Substring(spaces, currentScopeEnd + 1) + ";\r\n";
                    }
                    else
                    {
                        code += "    " + codeLines[i] + "\r\n";
                    }
                }
            }

            string leftover = codeLines[currentScopeEndLine].Trim().Substring(currentScopeEnd + 1);

            if (!codeLines[pragmaLine].EndsWith(" nosync") && !isPooled)
            {
                codeAfter = 
                    indent + "SYNCMASK" + tempIndex +"[" + currentTask + "] = true;\r\n";
            }

            writer.Write(codeBefore);
            writer.Write(code);
            writer.Write(codeAfter);

            writer.WriteLine(indent + leftover);

            for (int i = currentScopeEndLine + 1; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            writer.Close();
            currentTask++;
        }
    }
}