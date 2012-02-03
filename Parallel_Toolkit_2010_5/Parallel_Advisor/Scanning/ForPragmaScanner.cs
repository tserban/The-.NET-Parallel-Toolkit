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
    public class ForPragmaScanner : PragmaScanner
    {
        protected static string parallelFor = "//@ parallel for";
        protected ForTypes forType;
        protected int parameterValue = 0;
        protected string initialValue = null;
        protected string finalValue = null;
        protected string type = null;
        protected string forVariable = null;
        protected bool noSyncOn = false;
        protected Grammar forInstructionGrammar = null;
        protected static string grammarFile = @"Grammars\parallel_for_pragma_grammar.txt";

        /// <summary>
        /// Creates a new instance of class ForPragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public ForPragmaScanner(string file)
            : base(file, grammarFile)
        {
            forInstructionGrammar = new Grammar();
            forInstructionGrammar.ReadGrammarFromFile(@"Grammars\parallel_for_instruction_grammar.txt");
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
                            "Invalid parallel for pragma detected.\nUsage: //@ parallel for static/dynamic [iterationCount] [nosync]" + 
                            "\r\nwhere iterationCount >= 0.", WarningLevels.Fatal));
                        break;
                    case ErrorTypes.Valid:
                        ErrorTypes returnType = FindScope(codeLines, currentIndex + 1, "for", true);
                        switch (returnType)
                        {
                            case ErrorTypes.Invalid:
                                errorList.Add(new Error(currentLine + 1,
                                    "Invalid for statement detected.\nMust be of the form: "
                                    + "for (type value = startValue; value < endValue; value++), the for body must be "
                                    + "enclosed in curly braces and the type must be int or long.",
                                    WarningLevels.Fatal));
                                break;
                            case ErrorTypes.Valid:
                                ScopeScanner scanner = new ScopeScanner();
                                scanner.ParseString(scope);
                                ErrorTypes newReturnType = ValidateFor(scanner.ProgramInternalForm);
                                switch (newReturnType)
                                {
                                    case ErrorTypes.Invalid:
                                        errorList.Add(new Error(currentLine + 1,
                                            "Invalid for statement detected.\nMust be of the form: "
                                            + "for (type value = startValue; value < endValue; value++), the for body must be "
                                            + "enclosed in curly braces and the type must be int or long.",
                                            WarningLevels.Fatal));
                                        break;
                                    case ErrorTypes.Valid:
                                        Main.Application.PragmaLines.Add(currentLine - 1);
                                        errorList.AddRange(InspectScope(scanner.ProgramInternalForm, currentLine + 1));
                                        break;
                                }
                                break;
                            case ErrorTypes.Missing:
                                errorList.Add(new Error(currentLine + 1,
                                    "For statement missing.",
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
        /// Checks if a line contains a valid parallel for pragma.
        /// </summary>
        /// <param name="line">The line of code to analyze.</param>
        /// <returns>The return message.</returns>
        protected ErrorTypes CheckForPragmas(string line)
        {
            string trimmedLine = line.Trim();
            parameterValue = 0;

            if (trimmedLine.StartsWith(parallelFor))
            {
                List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(trimmedLine);
                
                if (!parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                {
                    return ErrorTypes.Invalid;
                }
                else
                {
                    List<PIFEntry> tokenizedPragma = PragmaTokenizer.TokenizePragma(trimmedLine);
                    if (tokenizedPragma[3].Symbol == "static")
                    {
                        forType = ForTypes.Static;
                    }
                    else
                    {
                        forType = ForTypes.Dynamic;
                    }

                    if (tokenizedPragma.Count > 4)
                    {
                        if(tokenizedPragma[4].Symbol == "nosync")
                        {
                            noSyncOn = true;
                        }
                        else
                        {
                            parameterValue = Int32.Parse(tokenizedPragma[4].Symbol);
                        }

                        if (tokenizedPragma.Count == 6)
                        {
                            noSyncOn = true;
                        }
                    }
                }

                return ErrorTypes.Valid;
            }

            return ErrorTypes.Missing;
        }

        /// <summary>
        /// Validates a for instruction.
        /// </summary>
        /// <param name="programInternalForm">The program internal form table.</param>
        /// <returns>Valid if the for is valid, Invalid otherwise.</returns>
        protected ErrorTypes ValidateFor(List<PIFEntry> programInternalForm)
        {
            List<string> tokens = new List<string>();

            foreach (PIFEntry entry in programInternalForm)
            {
                if (entry.Symbol == "{")
                {
                    break;
                }
                else
                {
                    if (entry.Code <= 1)
                    {
                        tokens.Add(entry.Code.ToString());
                    }
                    else
                    {
                        tokens.Add(entry.Symbol);
                    }
                }
            }


            if (!parser.ValidateExpression(tokens, forInstructionGrammar))
            {
                return ErrorTypes.Invalid;
            }

            // process component 1.
            tokens.Clear();
            int i = 2;
            while (programInternalForm[i].Symbol != ";")
            {
                tokens.Add(programInternalForm[i].Symbol);
                i++;
            }

            type = tokens[0];
            string variable1 = tokens[1];

            initialValue = "";
            for (int k = 5; k < i; k++)
            {
                initialValue += programInternalForm[k].Symbol;
            }

            // process component 2.
            int temp = i;
            i++;
            tokens.Clear();
            while (programInternalForm[i].Symbol != ";")
            {
                tokens.Add(programInternalForm[i].Symbol);
                i++;
            }

            string variable2 = tokens[0];

            if (variable2 != variable1)
            {
                return ErrorTypes.Invalid;
            }

            finalValue = "";
            for (int k = temp + 3; k < i; k++)
            {
                finalValue += programInternalForm[k].Symbol;
            }

            // process component 3.
            i++;
            tokens.Clear();
            while (programInternalForm[i].Symbol != "{")
            {
                tokens.Add(programInternalForm[i].Symbol);
                i++;
            }

            string variable3 = tokens[0];

            if (variable3 != variable1)
            {
                return ErrorTypes.Invalid;
            }

            return ErrorTypes.Valid;
        }

        /// <summary>
        /// Inspects a for loop scope and returns the list of errors encountered.
        /// </summary>
        /// <param name="programInternalForm">The program internal form obtained by scanning the for loop.</param>
        /// <param name="forLine">The line of the for statement.</param>
        /// <returns>The list of errors encountered.</returns>
        protected Error[] InspectScope(List<PIFEntry> programInternalForm, int forLine)
        {
            List<Error> errorList = new List<Error>();

            // begin separation of for scope.
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
            forVariable = programInternalForm.ElementAt(3).Symbol;

            // for scope begins at position i + 1 in the PIF.
            IEnumerable<PIFEntry> scopePIF = programInternalForm.Skip(i);
            Dictionary<int, string> variables = new Dictionary<int, string>();
            Dictionary<string, bool> isLocal = new Dictionary<string, bool>();

            foreach (PIFEntry entry in scopePIF)
            {
                if (entry.Code == 52)
                {
                    errorList.Add(new Error(forLine,
                        "Abrupt exit by 'throw' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.Fatal));
                }

                if (entry.Code == 53)
                {
                    errorList.Add(new Error(forLine,
                        "Possibly abrupt exit by 'break' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.NonFatal));
                }

                if (entry.Code == 74)
                {
                    errorList.Add(new Error(forLine,
                        "Possibly abrupt exit by 'goto' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.NonFatal));
                }

                if (entry.Code == 87)
                {
                    errorList.Add(new Error(forLine,
                        "Abrupt exit by 'return' keyword detected. This could lead to unexpected behaviour.",
                        WarningLevels.Fatal));
                }
            }

            foreach (PIFEntry entry in scopePIF)
            {
                if (entry.Code == 0 && entry.Symbol != forVariable)
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

                if (programInternalForm[index + 1].Symbol == "[")
                {
                    List<PIFEntry> indexing = ParseScope(programInternalForm, index + 1, "[", "]");
                    int j = 0;
                    foreach (PIFEntry entry in indexing)
                    {
                        j++;
                        if (entry.Symbol == forVariable && 
                            (programInternalForm[index + 1 + j + 1].Symbol == "-" || programInternalForm[index + 1 + j + 1].Symbol == "+"))
                        {
                            errorList.Add(new Error(forLine,
                                "Possible loop interdependence detected. This will inhibit parallel execution."));
                            continue;
                        }
                    }
                }

                Error error = VerifyVariable(variable, programInternalForm, index, forLine, isLocal);
                if (error != null)
                {
                    errorList.Add(error);
                }
            }

            return errorList.ToArray();
        }
    }
}
