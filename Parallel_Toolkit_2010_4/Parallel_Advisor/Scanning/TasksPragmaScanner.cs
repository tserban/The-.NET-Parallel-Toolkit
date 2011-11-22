using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel_Advisor.Errors;
using System.IO;
using Parallel_Advisor.Util;
using System.Windows;
using Parallel_Advisor.Parsing;

namespace Parallel_Advisor.Scanning
{
    public class TasksPragmaScanner : PragmaScanner
    {
        protected static string parallelTasks = "//@ parallel tasks";
        protected static string endParallelTasks = "//@ end tasks";
        protected static string parallelTask = "//@ parallel task";
        protected List<int> validParallelTasks = new List<int>();
        protected int endTasksLine = 0;       
        protected bool isPooled = false;
        protected bool noSyncOn = false;
        protected Grammar taskPragmaGrammar = null;
        protected static string grammarFile = @"Grammars\parallel_tasks_pragma_grammar.txt";

        /// <summary>
        /// Creates a new instance of class TasksPragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public TasksPragmaScanner(string file)
            : base(file, grammarFile)
        {
            taskPragmaGrammar = new Grammar();
            taskPragmaGrammar.ReadGrammarFromFile(@"Grammars\parallel_task_pragma_grammar.txt");
        }

        /// <summary>
        /// Scans the given file and returns the errors that have occured.
        /// </summary>
        /// <returns>The list of errors that have occured during scanning.</returns>
        public override Error[] ScanFile()
        {
            List<Error> errorList = new List<Error>();
            List<string> temp = new List<string>();
            int lineCounter = 0;

            TextReader reader = null;

            try
            {
                reader = new StreamReader(SourceFile);

                string line = reader.ReadLine();
                while (line != null)
                {
                    lineCounter++;
                    temp.Add(line);
                    line = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logging.Log(ex.ToString());
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            string[] codeLines = temp.ToArray<string>();
            int currentIndex = 0;

            while (currentIndex < lineCounter)
            {
                int currentLine = currentIndex + 1;
                string line = codeLines[currentIndex];
                ErrorTypes errorType = CheckForPragmas(currentIndex, codeLines);

                switch (errorType)
                {
                    case ErrorTypes.Invalid:
                        errorList.Add(new Error(currentLine,
                            "Invalid parallel tasks pragma detected.\nUsage: //@ parallel tasks [pooled] [nosync] and it must end with //@ end tasks", 
                            WarningLevels.Fatal));
                        break;
                    case ErrorTypes.Valid:
                        validParallelTasks.Clear();
                        errorList.AddRange(ValidateTasks(currentIndex, codeLines));
                        Main.Application.PragmaLines.Add(currentLine - 1);
                        if (validParallelTasks.Count > 0)
                        {
                            foreach (int validTaskLine in validParallelTasks)
                            {
                                ErrorTypes returnType = FindScope(codeLines, validTaskLine + 1, "{", false);
                                switch (returnType)
                                {
                                    case ErrorTypes.Invalid:
                                        errorList.Add(new Error(validTaskLine + 2,
                                            "Invalid parallel task detected.\nMust be a scope enclosed in curly braces.",
                                            WarningLevels.Fatal));
                                        break;
                                    case ErrorTypes.Valid:
                                        ScopeScanner scanner = new ScopeScanner();
                                        scanner.ParseString(scope);
                                        errorList.AddRange(InspectScope(scanner.ProgramInternalForm, validTaskLine + 1));
                                        break;
                                    case ErrorTypes.Missing:
                                        errorList.Add(new Error(validTaskLine + 2,
                                            "Parallel task missing.",
                                            WarningLevels.Fatal));
                                        break;
                                }
                            }
                        }
                        break;
                }

                currentIndex++;
            }

            return errorList.ToArray<Error>();
        }

        /// <summary>
        /// Checks if a line contains a valid parallel while pragma.
        /// </summary>
        /// <param name="currentIndex">The index of this line.</param>
        /// /// <param name="currentIndex">The code lines.</param>
        /// <returns>The return message.</returns>
        protected ErrorTypes CheckForPragmas(int currentIndex, string[] codeLines)
        {
            string trimmedLine = codeLines[currentIndex].Trim();

            if (trimmedLine.StartsWith(parallelTasks))
            {
                List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(trimmedLine);

                if (parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                {
                    if (pragmaSplit.Count == 5)
                    {
                        isPooled = true;
                        noSyncOn = true;
                    }
                    else
                    {
                        if (pragmaSplit.Count == 4)
                        {
                            if (pragmaSplit[3] == "nosync")
                            {
                                noSyncOn = true;
                            }
                            else
                            {
                                isPooled = true;
                            }
                        }
                    }

                    int count = 1;
                    int i = currentIndex + 1;
                    while (i < codeLines.Length)
                    {
                        if (codeLines[i].Trim() == endParallelTasks)
                        {
                            count--;
                            if (count == 0)
                            {
                                endTasksLine = i;
                                return ErrorTypes.Valid;
                            }
                        }
                        else
                        {
                            List<string> newPragmaSplit = PragmaTokenizer.ReturnParseSequence(codeLines[i].Trim());
                            if (parser.ValidateExpression(newPragmaSplit, pragmaGrammar))
                            {
                                count++;
                            }
                        }
                        i++;
                    }

                    if (i == codeLines.Length)
                    {
                        return ErrorTypes.Invalid;
                    }
                }
                else
                {
                    return ErrorTypes.Invalid;
                }
            }

            return ErrorTypes.Missing;
        }

        /// <summary>
        /// Validates the parallel tasks encountered in the source code.
        /// </summary>
        /// <param name="startIndex">The start index of the parallel tasks region.</param>
        /// <param name="codeLines">The lines of code.</param>
        /// <returns>The list of errors encountered.</returns>
        protected Error[] ValidateTasks(int startIndex, string[] codeLines)
        {
            List<Error> errors = new List<Error>();

            int i = startIndex + 1;

            while (i < endTasksLine)
            {
                string line = codeLines[i].Trim();
                if (line.StartsWith(parallelTask))
                {
                    List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(line);
                    if (!parser.ValidateExpression(pragmaSplit, taskPragmaGrammar))
                    {
                        if (!parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                        {
                            errors.Add(new Error(i + 1, "Invalid parallel task pragma detected.\nUsage: //@ parallel task [nosync]", WarningLevels.Fatal));
                        }
                    }
                    else
                    {
                        validParallelTasks.Add(i);
                    }
                }
                i++;
            }

            return errors.ToArray();
        }

        /// <summary>
        /// Inspects a parallel task scope and returns the list of errors encountered.
        /// </summary>
        /// <param name="programInternalForm">The program internal form obtained by scanning the task scope.</param>
        /// <param name="forLine">The line of the task start.</param>
        /// <returns>The list of errors encountered.</returns>
        protected Error[] InspectScope(List<PIFEntry> programInternalForm, int taskLine)
        {
            List<Error> errorList = new List<Error>();

            int i = 1;

            // task scope begins at position i + 1 in the PIF.
            IEnumerable<PIFEntry> scopePIF = programInternalForm.Skip(i);
            Dictionary<int, string> variables = new Dictionary<int, string>();
            Dictionary<string, bool> isLocal = new Dictionary<string, bool>();

            foreach (PIFEntry entry in scopePIF)
            {
                if (entry.Code == 0)
                {
                    variables.Add(i, entry.Symbol);
                    try
                    {
                        isLocal.Add(entry.Symbol, false);
                    }
                    catch { }
                }
                i++;
            }

            // now the Dictionary variables contains all the appearances of each variable or custom type.
            foreach (KeyValuePair<int, string> pair in variables)
            {
                string variable = pair.Value;
                int index = pair.Key;

                Error error = VerifyVariable(variable, programInternalForm, index, taskLine, isLocal);
                if (error != null)
                {
                    errorList.Add(error);
                }
            }

            return errorList.ToArray();
        }
    }
}
