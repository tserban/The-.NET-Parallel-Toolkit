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
    public class WhilePragmaScanner : PragmaScanner
    {
        protected static string parallelWhile = "//@ parallel while";
        protected bool noSyncOn = false;
        protected static string grammarFile = @"Grammars\parallel_while_pragma_grammar.txt";

        /// <summary>
        /// Creates a new instance of class WhilePragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public WhilePragmaScanner(string file)
            : base(file, grammarFile)
        {
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
                ErrorTypes errorType = CheckForPragmas(line);

                switch (errorType)
                {
                    case ErrorTypes.Invalid:
                        errorList.Add(new Error(currentLine,
                            "Invalid parallel while pragma detected.\nUsage: //@ parallel while [nosync]"
                            , WarningLevels.Fatal));
                        break;
                    case ErrorTypes.Valid:
                        ErrorTypes returnType = FindScope(codeLines, currentIndex + 1, "while", true);
                        switch (returnType)
                        {
                            case ErrorTypes.Invalid:
                                errorList.Add(new Error(currentLine + 1,
                                    "Invalid while statement detected.\nMust be of the form: "
                                    + "while(condition) and the while body must be enclosed in curly braces.",
                                    WarningLevels.Fatal));
                                break;
                            case ErrorTypes.Valid:
                                ScopeScanner scanner = new ScopeScanner();
                                scanner.ParseString(scope);
                                errorList.AddRange(InspectScope(scanner.ProgramInternalForm, currentLine + 1));
                                Main.Application.PragmaLines.Add(currentLine - 1);
                                break;
                            case ErrorTypes.Missing:
                                errorList.Add(new Error(currentLine + 1,
                                    "While statement missing.",
                                    WarningLevels.Fatal));
                                break;
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
        /// <param name="line">The line of code to analyze.</param>
        /// <returns>The return message.</returns>
        protected ErrorTypes CheckForPragmas(string line)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith(parallelWhile))
            {
                List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(trimmedLine);

                if (!parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                {
                    return ErrorTypes.Invalid;
                }
                else
                {
                    if (pragmaSplit.Count == 4)
                    {
                        noSyncOn = true;
                    }
                    return ErrorTypes.Valid;
                }
            }

            return ErrorTypes.Missing;
        }

        /// <summary>
        /// Inspects a while scope and returns the list of errors encountered.
        /// </summary>
        /// <param name="programInternalForm">The program internal form obtained by scanning the while loop.</param>
        /// <param name="forLine">The line of the while start.</param>
        /// <returns>The list of errors encountered.</returns>
        protected Error[] InspectScope(List<PIFEntry> programInternalForm, int whileLine)
        {
            List<Error> errorList = new List<Error>();

            // begin separation of while scope.
            int paranthesisCount = 0;
            int i = 0;

            foreach (PIFEntry entry in programInternalForm)
            {
                if (entry.Symbol == "(")
                {
                    paranthesisCount++;
                }
                else
                {
                    if (entry.Symbol == ")")
                    {
                        paranthesisCount--;
                        if (paranthesisCount == 0)
                        {
                            break;
                        }
                    }
                }

                i++;
            }

            i++;

            // while scope begins at position i + 1 in the PIF.
            IEnumerable<PIFEntry> scopePIF = programInternalForm.Skip(i);
            Dictionary<int, string> variables = new Dictionary<int, string>();
            Dictionary<string, bool> isLocal = new Dictionary<string, bool>();

            foreach (PIFEntry entry in scopePIF)
            {
                if (entry.Code == 52)
                {
                    errorList.Add(new Error(whileLine,
                        "Abrupt exit by 'throw' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.Fatal));
                }

                if (entry.Code == 53)
                {
                    errorList.Add(new Error(whileLine,
                        "Possibly abrupt exit by 'break' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.NonFatal));
                }

                if (entry.Code == 74)
                {
                    errorList.Add(new Error(whileLine,
                        "Possibly abrupt exit by 'goto' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.NonFatal));
                }

                if (entry.Code == 87)
                {
                    errorList.Add(new Error(whileLine,
                        "Abrupt exit by 'return' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.Fatal));
                }
            }

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

                Error error = VerifyVariable(variable, programInternalForm, index, whileLine, isLocal);
                if (error != null)
                {
                    errorList.Add(error);
                }
            }

            return errorList.ToArray();
        }
    }
}
