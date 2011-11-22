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
    public class AtomicPragmaScanner : PragmaScanner
    {
        protected static string parallelAtomic = "//@ parallel atomic";
        protected string variable = null;
        protected string expression = null;
        protected OperationTypes opType;
        protected static string grammarFile = @"Grammars\parallel_atomic_pragma_grammar.txt";
        protected Grammar opGrammar = null;

        protected enum OperationTypes : int
        {
            Addition,
            Subtraction,
            Increment,
            Decrement
        }

        /// <summary>
        /// Creates a new instance of class AtomicPragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public AtomicPragmaScanner(string file)
            : base(file, grammarFile)
        {
            opGrammar = new Grammar();
            opGrammar.ReadGrammarFromFile(@"Grammars\parallel_atomic_op_grammar.txt");
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
                            "Invalid parallel atomic pragma detected.\nUsage: //@ parallel atomic",
                            WarningLevels.Fatal));
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
                                        errorList.Add(new Error(currentLine + 1,
                                            "Invalid atomic operation detected.\nMust be a single instruction and either " +
                                            "increment, decrement, addition or subtraction.",
                                            WarningLevels.Fatal));
                                        break;
                                    case ErrorTypes.Valid:
                                        Main.Application.PragmaLines.Add(currentLine - 1);
                                        break;
                                }
                                break;
                            case ErrorTypes.Missing:
                                errorList.Add(new Error(currentLine + 1,
                                    "Atomic instruction missing.",
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
        /// Checks if a line contains a valid parallel atomic pragma.
        /// </summary>
        /// <param name="line">The line of code to analyze.</param>
        /// <returns>The return message.</returns>
        protected ErrorTypes CheckForPragmas(string line)
        {
            string trimmedLine = line.Trim();

            List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(trimmedLine);
            if (trimmedLine.StartsWith(parallelAtomic))
            {
                if (parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                {
                    return ErrorTypes.Valid;
                }
                return ErrorTypes.Invalid;
            }

            return ErrorTypes.Missing;
        }

        /// <summary>
        /// Retrieves the instruction corresponding to an atomic pragma.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="currentLine">The index of the line where the atomic pragma was found.</param>
        /// <returns>The error type encountered.</returns>
        protected ErrorTypes FindAtomic(string[] codeLines, int currentIndex)
        {
            currentScopeStartLine = currentIndex;

            if (currentIndex < codeLines.Length)
            {
                int currentLine = currentIndex;
                string trimmedLine = codeLines[currentLine].Trim();

                if (!trimmedLine.StartsWith("//") && !trimmedLine.StartsWith("/*"))
                {
                    int i = 0;
                    StringBuilder builder = new StringBuilder("");

                search:
                    trimmedLine = codeLines[currentLine].Trim();

                    if (i == trimmedLine.Length && !trimmedLine.Contains(";"))
                    {
                        return ErrorTypes.Missing;
                    }
                    else
                    {
                        if (trimmedLine[i] == '"' && trimmedLine[i - 1] != '\\' && trimmedLine[i - 1] != '\'')
                        {
                        parseString:
                            builder.Append(trimmedLine[i]);
                            i++;
                            while (trimmedLine[i] != '"')
                            {
                                builder.Append(trimmedLine[i]);
                                i++;
                            }

                            builder.Append(trimmedLine[i]);
                            int j = i - 1;
                            while (trimmedLine[j] == '\\')
                            {
                                j--;
                            }

                            if ((i - j + 1) % 2 == 1)
                            {
                                goto parseString;
                            }
                            else
                            {
                                i++;
                                goto search;
                            }
                        }

                        if (trimmedLine[i] == '/' && (trimmedLine[i + 1] == '/' || trimmedLine[i + 1] == '*'))
                        {
                            if (trimmedLine[i + 1] == '/')
                            {
                                return ErrorTypes.Missing;
                            }
                            else
                            {
                                Parallel_Advisor.Scanning.Point continuePoint = ParseMultilineComment(codeLines,
                                    new Parallel_Advisor.Scanning.Point(currentLine, i));
                                i = continuePoint.Column + 1;
                                if (currentLine != continuePoint.Row)
                                {
                                    return ErrorTypes.Missing;
                                }
                                else
                                {
                                    goto search;
                                }
                            }
                        }
                        else
                        {
                            if (i == trimmedLine.Length - 1)
                            {
                                builder.Append(trimmedLine[i] + " ");
                            }
                            else
                            {
                                builder.Append(trimmedLine[i]);
                            }

                            if (trimmedLine[i] == ';')
                            {
                                scope = builder.ToString();
                                currentScopeEnd = i;
                                currentScopeEndLine = currentLine;
                                return ErrorTypes.Valid;
                            }

                            i++;
                            goto search;
                        }
                    }
                }
                else
                {
                    return ErrorTypes.Missing;
                }
            }
            else
            {
                return ErrorTypes.Missing;
            }
        }

        /// <summary>
        /// Validates an atomic instruction.
        /// </summary>
        /// <param name="programInternalForm">The program internal form.</param>
        /// <returns>The error encountered.</returns>
        protected ErrorTypes ValidateAtomic(List<PIFEntry> programInternalForm)
        {
            List<string> tokens = new List<string>();
            string op = null;
            foreach (PIFEntry entry in programInternalForm)
            {
                if (entry.Code <= 1)
                {
                    tokens.Add(entry.Code.ToString());
                }
                else
                {
                    tokens.Add(entry.Symbol);
                }

                if (entry.Symbol == "}")
                {
                    return ErrorTypes.Invalid;
                }

                if (ScopeScanner.AssignmentOperators.Contains(entry.Symbol))
                {
                    op = entry.Symbol;
                    break;
                }
            }

            if (!parser.ValidateExpression(tokens, opGrammar))
            {
                return ErrorTypes.Invalid;
            }

            if (op == "+=")
            {
                opType = OperationTypes.Addition;
            }

            if (op == "-=")
            {
                opType = OperationTypes.Subtraction;
            }

            if (op == "++")
            {
                opType = OperationTypes.Increment;
            }

            if (op == "--")
            {
                opType = OperationTypes.Decrement;
            }

            StringBuilder variableBuilder = new StringBuilder();
            int i = 0;

            while (programInternalForm[i].Symbol != op)
            {
                variableBuilder.Append(programInternalForm[i].Symbol);
                i++;
            }

            i++;
            variable = variableBuilder.ToString();
            StringBuilder expressionBuilder = new StringBuilder();

            while (programInternalForm[i].Symbol != ";" && programInternalForm[i].Symbol != ",")
            {
                expressionBuilder.Append(programInternalForm[i].Symbol);
                i++;
            }

            expression = expressionBuilder.ToString();

            return ErrorTypes.Valid;
        }
    }
}
