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
    public class ParallelForConverter : ForPragmaScanner, IConverter
    {
        private int forLoopIndex = 0;
        private int pragmaLine = 0;

        /// <summary>
        /// Creates a new instance of class ParallelForConverter.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ParallelForConverter(string file)
            : base(file)
        {
        }

        /// <summary>
        /// Scans the given source code and converts it into parallel code.
        /// </summary>
        /// <param name="outputFile">The file that contains the converted code.</param>
        public bool ScanAndConvert(string outputFile)
        {
            noSyncOn = false;
            string[] codeLines = ReadContent(SourceFile);
            int currentIndex = 0;
            int lineCounter = codeLines.Length;

            if (codeLines[codeLines.Length - 1].StartsWith(
                "// -embedded line, required for future conversions, do not alter- "))
            {
                if (Int32.TryParse(codeLines[codeLines.Length - 1].Substring(
                    "// -embedded line, required for future conversions, do not alter- ".Length), out forLoopIndex))
                {
                }
                else
                {
                    forLoopIndex = 0;
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
                        ErrorTypes returnType = FindScope(codeLines, currentIndex + 1, "for", true);
                        switch (returnType)
                        {
                            case ErrorTypes.Invalid:
                                break;
                            case ErrorTypes.Valid:
                                pragmaLine = currentLine;
                                ScopeScanner scanner = new ScopeScanner();
                                scanner.ParseString(scope);
                                ErrorTypes newReturnType = ValidateFor(scanner.ProgramInternalForm);
                                switch (newReturnType)
                                {
                                    case ErrorTypes.Invalid:
                                        break;
                                    case ErrorTypes.Valid:
                                        InspectScope(scanner.ProgramInternalForm, currentLine + 1);
                                        ConvertFor(codeLines, outputFile);
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
        /// Converts a for loop to a parallel for loop.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="parameterValue">The iteration count, 0 means continuous allocation.</param>
        /// <param name="outputFile"></param>
        private void ConvertFor(string[] codeLines, string outputFile)
        {
            forLoopIndex++;
            TextWriter writer = new StreamWriter(outputFile);

            for (int i = 0; i < pragmaLine - 1; i++)
            {
                writer.WriteLine(codeLines[i]);
            }

            int indentation = codeLines[pragmaLine].TrimEnd().Length - codeLines[pragmaLine].Trim().Length;
            string indent = new string(' ', indentation);

            string codeBefore = null;
            string codeAfter = null;
            string code = null;

            if (forType == ForTypes.Static)
            {
                codeBefore =
                        indent + "int CPUS" + forLoopIndex + " = Environment.ProcessorCount;\r\n" +
                        indent + "CPUS" + forLoopIndex + " = CPUS" + forLoopIndex + " < (" + finalValue + " - " + initialValue + ") ? CPUS" + forLoopIndex + " : (" + finalValue + " - " + initialValue + ");\r\n" +
                        indent + "System.Threading.Thread[] THREADS" + forLoopIndex + " = new System.Threading.Thread[CPUS" + forLoopIndex + "];\r\n" +
                        indent + "for (int TID" + forLoopIndex + " = 0; TID" + forLoopIndex + " < CPUS" + forLoopIndex + "; TID" + forLoopIndex + "++)\r\n" +
                        indent + "{\r\n" +
                        indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "] = new System.Threading.Thread(\r\n" +
                        indent + "        PARAM" + forLoopIndex + " =>\r\n" +
                        indent + "        {\r\n";

                if (parameterValue == 0)
                {
                    codeBefore +=
                        indent + "            int ID" + forLoopIndex + " = (int)PARAM" + forLoopIndex + ";\r\n" +
                        indent + "            " + type + " REMAINDER" + forLoopIndex + " = (" + finalValue + " - " + initialValue + ") % CPUS" + forLoopIndex + ";\r\n\r\n" +

                        indent + "            " + type + " START" + forLoopIndex + " = ID" + forLoopIndex + " * ((" + finalValue + " - " + initialValue + ") / CPUS" + forLoopIndex + ") + " + initialValue + ";\r\n" +
                        indent + "            " + type + " END" + forLoopIndex + " = (ID" + forLoopIndex + " + 1) * ((" + finalValue + " - " + initialValue + ") / CPUS" + forLoopIndex + ") + " + initialValue + ";\r\n\r\n" +

                        indent + "            if (ID" + forLoopIndex + " < REMAINDER" + forLoopIndex + ")\r\n" +
                        indent + "            {\r\n" +
                        indent + "                START" + forLoopIndex + " += ID" + forLoopIndex + ";\r\n" +
                        indent + "                END" + forLoopIndex + " += ID" + forLoopIndex + " + 1;\r\n" +
                        indent + "            }\r\n" +
                        indent + "            else\r\n" +
                        indent + "            {\r\n" +
                        indent + "                START" + forLoopIndex + " += REMAINDER" + forLoopIndex + ";\r\n" +
                        indent + "                END" + forLoopIndex + " += REMAINDER" + forLoopIndex + ";\r\n" +
                        indent + "            }\r\n\r\n";

                    code = indent + "            for (" + type + " " + forVariable + " = START" + forLoopIndex + "; " +
                        forVariable + " < END" + forLoopIndex + "; " + forVariable + "++)";

                    if (currentScopeStartLine == currentScopeEndLine)
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1, currentScopeEnd - instructionEnds) + "\r\n";
                    }
                    else
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1) + "\r\n";

                        for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                        {
                            if (i == currentScopeEndLine)
                            {
                                int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                                string spacing = new string(' ', spaces);
                                code += spacing + "            " + codeLines[i].Substring(spaces, currentScopeEnd + 1) + "\r\n";
                            }
                            else
                            {
                                code += "            " + codeLines[i] + "\r\n";
                            }
                        }
                    }
                }
                else
                {
                    codeBefore +=
                        indent + "            int ID" + forLoopIndex + " = (int)PARAM" + forLoopIndex + ";\r\n" +
                        indent + "            int GAP" + forLoopIndex + " = " + parameterValue + ";\r\n" +
                        indent + "            " + type + " START" + forLoopIndex + " = ID" + forLoopIndex + " * GAP" + forLoopIndex + " + " + initialValue + ";\r\n" +
                        indent + "            " + type + " END" + forLoopIndex + " = " + finalValue + ";\r\n\r\n" +
                        indent + "            " + "for (" + type + " INDEX" + forLoopIndex + " = START" + forLoopIndex + "; INDEX" + forLoopIndex + " < END" + forLoopIndex + "; INDEX" + forLoopIndex + " += CPUS" + forLoopIndex + " * GAP" + forLoopIndex + ")\r\n" +
                        indent + "            " + "{\r\n";

                    code = indent + "                for (" + type + " " + forVariable + " = INDEX" + forLoopIndex + "; " +
                        forVariable + " < INDEX" + forLoopIndex + " + GAP" + forLoopIndex + " && " + forVariable + " < END" +
                        forLoopIndex + "; " + forVariable + "++)";

                    if (currentScopeStartLine == currentScopeEndLine)
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1, currentScopeEnd - instructionEnds) + "\r\n";
                    }
                    else
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1) + "\r\n";

                        for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                        {
                            if (i == currentScopeEndLine)
                            {
                                int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                                string spacing = new string(' ', spaces);
                                code += spacing + "                " + codeLines[i].Substring(spaces, currentScopeEnd + 1) + "\r\n";
                            }
                            else
                            {
                                code += "                " + codeLines[i] + "\r\n";
                            }
                        }
                    }

                    codeAfter = indent + "            }\r\n";
                }

                codeAfter +=
                     indent + "        });\r\n" +
                     indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "].Start(TID" + forLoopIndex + ");\r\n" +
                     indent + "}\r\n\r\n";

                if (!noSyncOn)
                {
                    codeAfter +=
                         indent + "for (int TID" + forLoopIndex + " = 0; TID" + forLoopIndex + " < CPUS" + forLoopIndex + "; TID" + forLoopIndex + "++)\r\n" +
                         indent + "{\r\n" +
                         indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "].Join();\r\n" +
                         indent + "}\r\n\r\n";
                }
            }
            else
            {
                codeBefore =
                    indent + "int CPUS" + forLoopIndex + " = Environment.ProcessorCount;\r\n" +
                    indent + "CPUS" + forLoopIndex + " = CPUS" + forLoopIndex + " < (" + finalValue + " - " + initialValue + ") ? CPUS" + forLoopIndex + " : (" + finalValue + " - " + initialValue + ");\r\n" +
                    indent + "System.Threading.Thread[] THREADS" + forLoopIndex + " = new System.Threading.Thread[CPUS" + forLoopIndex + "];\r\n" +
                    indent + "for (int TID" + forLoopIndex + " = 0; TID" + forLoopIndex + " < CPUS" + forLoopIndex + "; TID" + forLoopIndex + "++)\r\n" +
                    indent + "{\r\n" +
                    indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "] = new System.Threading.Thread(\r\n" +
                    indent + "        PARAM" + forLoopIndex + " =>\r\n" +
                    indent + "        {\r\n";

                if (parameterValue == 0)
                {
                    codeBefore = codeBefore.Insert(0, indent + type + " CURRENT_TASK" + forLoopIndex + " = " + initialValue + " - 1;\r\n");
                    codeBefore +=
                        indent + "            " + type + " " + forVariable + " = 0;\r\n" + 
                        indent + "            while((" + forVariable + " = System.Threading.Interlocked.Increment(ref CURRENT_TASK" + forLoopIndex + ")) < " + finalValue + ")\r\n" +
                        indent + "            {";

                    if (currentScopeStartLine == currentScopeEndLine)
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1, currentScopeEnd - instructionEnds) + "\r\n";
                    }
                    else
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1) + "\r\n";

                        for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                        {
                            if (i == currentScopeEndLine)
                            {
                                int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                                string spacing = new string(' ', spaces);
                                code += spacing + "                " + codeLines[i].Substring(spaces, currentScopeEnd + 1) + "\r\n";
                            }
                            else
                            {
                                code += "                " + codeLines[i] + "\r\n";
                            }
                        }
                    }
                }
                else
                {
                    codeBefore = codeBefore.Insert(0, indent + type + " GAP" + forLoopIndex + " = " + parameterValue + ";\r\n" +
                        indent + type + " CURRENT_TASK" + forLoopIndex + " = " + initialValue + " - GAP" + forLoopIndex + ";\r\n");
                    codeBefore +=
                        indent + "            " + type + " START" + forLoopIndex + " = 0;\r\n" +
                        indent + "            " + type + " END" + forLoopIndex + " = 0;\r\n" +
                        indent + "            while((START" + forLoopIndex + " = System.Threading.Interlocked.Add(ref CURRENT_TASK" + forLoopIndex + ", GAP" + forLoopIndex + ")) < " + finalValue + ")\r\n" +
                        indent + "            {\r\n" +
                        indent + "                END" + forLoopIndex + " = START" + forLoopIndex + " + GAP" + forLoopIndex + ";\r\n" +
                        indent + "                for (" + type + " " + forVariable + " = START" + forLoopIndex + "; " + forVariable + " < END" + forLoopIndex + " && " + forVariable + " < " + finalValue + "; " + forVariable + "++)";

                    if (currentScopeStartLine == currentScopeEndLine)
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1, currentScopeEnd - instructionEnds) + "\r\n";
                    }
                    else
                    {
                        code += codeLines[currentScopeStartLine].Trim().Substring(instructionEnds + 1) + "\r\n";

                        for (int i = currentScopeStartLine + 1; i <= currentScopeEndLine; i++)
                        {
                            if (i == currentScopeEndLine)
                            {
                                int spaces = codeLines[i].Length - codeLines[i].Trim().Length;
                                string spacing = new string(' ', spaces);
                                code += spacing + "                " + codeLines[i].Substring(spaces, currentScopeEnd + 1) + "\r\n";
                            }
                            else
                            {
                                code += "                " + codeLines[i] + "\r\n";
                            }
                        }
                    }
                }

                codeAfter +=
                     indent + "            }\r\n" +
                     indent + "        });\r\n" +
                     indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "].Start();\r\n" +
                     indent + "}\r\n\r\n";

                if (!noSyncOn)
                {
                    codeAfter +=
                         indent + "for (int TID" + forLoopIndex + " = 0; TID" + forLoopIndex + " < CPUS" + forLoopIndex + "; TID" + forLoopIndex + "++)\r\n" +
                         indent + "{\r\n" +
                         indent + "    THREADS" + forLoopIndex + "[TID" + forLoopIndex + "].Join();\r\n" +
                         indent + "}\r\n\r\n";
                }
            }

            string leftover = codeLines[currentScopeEndLine].Trim().Substring(currentScopeEnd + 1);

            StringBuilder region = new StringBuilder();
            region.Append("\r\n" + indent + "#region Parallel Section " + forLoopIndex + " parallel for\r\n\r\n");
            region.Append(codeBefore);
            region.Append(code);
            region.Append(codeAfter);
            region.Append("\r\n" + indent + "#endregion\r\n\r\n");

            writer.Write(region.ToString());

            writer.WriteLine(indent + leftover);

            if (codeLines[codeLines.Length - 1] ==
                "// -embedded line, required for future conversions, do not alter- " + (forLoopIndex - 1))
            {
                codeLines[codeLines.Length - 1] = "// -embedded line, required for future conversions, do not alter- " + (forLoopIndex);
            }

            for (int i = currentScopeEndLine + 1; i < codeLines.Length; i++)
            {
                writer.Write(codeLines[i] + "\r\n");
            }

            if (codeLines[codeLines.Length - 1] !=
                "// -embedded line, required for future conversions, do not alter- " + forLoopIndex)
            {
                writer.Write("// -embedded line, required for future conversions, do not alter- " + forLoopIndex);
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