using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel_Advisor.Errors;
using System.IO;
using Parallel_Advisor.Util;
using Parallel_Advisor.Parsing;

namespace Parallel_Advisor.Scanning
{
    public abstract class PragmaScanner
    {
        public string SourceFile { get; set; }
        protected int currentScopeStartLine = 0;
        protected int currentScopeEndLine = 0;
        protected int currentScopeEnd = 0;
        protected int instructionEnds = 0;
        protected string scope = null;
        protected Grammar pragmaGrammar = null;
        protected Parser parser = null;

        /// <summary>
        /// Creates a new instance of class PragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public PragmaScanner(string file, string grammarFile)
        {
            SourceFile = file;
            pragmaGrammar = new Grammar();
            pragmaGrammar.ReadGrammarFromFile(grammarFile);
            parser = new Parser();
        }

        /// <summary>
        /// Scans the given file and returns the errors that have occured.
        /// </summary>
        /// <returns>The list of errors that have occured during scanning.</returns>
        public abstract Error[] ScanFile();

        /// <summary>
        /// Determines whether a valid for statement is found on the given index and retrieves its scope.
        /// </summary>
        /// <param name="codeLines">The source code lines.</param>
        /// <param name="currentLine">The index of the line where the for statement was found.</param>
        /// <returns>The error type encountered.</returns>
        protected ErrorTypes FindScope(string[] codeLines, int currentIndex, string startsWith, bool isLoop)
        {
            if (isLoop)
            {
                currentScopeStartLine = 0;
            }
            else
            {
                currentScopeStartLine = currentIndex;
            }

            if (currentIndex < codeLines.Length)
            {
                int currentLine = currentIndex;
                string trimmedLine = codeLines[currentLine].Trim();

                if (trimmedLine.StartsWith(startsWith))
                {
                    int i = 0;
                    StringBuilder builder = new StringBuilder("");
                    if (isLoop)
                    {
                        i = startsWith.Length;
                        builder = new StringBuilder(startsWith);
                    }
                    int curlyBracesCount = 0;
                    int paranthesisCount = 0;

                search:
                    trimmedLine = codeLines[currentLine].Trim();

                    if (i == trimmedLine.Length)
                    {
                        i = 0;
                        currentLine++;
                        goto search;
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
                                i = 0;
                                currentLine++;
                                goto search;
                            }
                            else
                            {
                                Parallel_Advisor.Scanning.Point continuePoint = ParseMultilineComment(codeLines,
                                    new Parallel_Advisor.Scanning.Point(currentLine, i));
                                i = continuePoint.Column + 1;
                                currentLine = continuePoint.Row;
                                goto search;
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

                            if (trimmedLine[i] == '(')
                            {
                                paranthesisCount++;
                            }

                            if (trimmedLine[i] == ')')
                            {
                                paranthesisCount--;
                                if (paranthesisCount == 0)
                                {
                                    if (currentScopeStartLine == 0)
                                    {
                                        instructionEnds = i;
                                        currentScopeStartLine = currentLine;
                                    }
                                }
                            }

                            if (paranthesisCount == 0 && curlyBracesCount == 0 && trimmedLine[i] == ';')
                            {
                                return ErrorTypes.Invalid;
                            }

                            if (trimmedLine[i] == '{')
                            {
                                curlyBracesCount++;
                            }

                            if (trimmedLine[i] == '}')
                            {
                                curlyBracesCount--;
                                if (curlyBracesCount == 0)
                                {
                                    scope = builder.ToString();
                                    currentScopeEnd = i;
                                    currentScopeEndLine = currentLine;
                                    return ErrorTypes.Valid;
                                }
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
        /// Reads the contents of a file and returns the string array representing the lines of the file.
        /// </summary>
        /// <param name="SourceFile">The source file to read.</param>
        /// <returns>The string array representing the lines of the file.</returns>
        public string[] ReadContent(string SourceFile)
        {
            List<string> temp = new List<string>();
            TextReader reader = null;

            try
            {
                reader = new StreamReader(SourceFile);

                string line = reader.ReadLine();
                while (line != null)
                {
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

            return temp.ToArray<string>();
        }

        /// <summary>
        /// Parses a multiline comment until the end.
        /// </summary>
        /// <param name="start">The start of the comment.</param>
        /// <returns>The end of the comment.</returns>
        protected Point ParseMultilineComment(string[] codeLines, Point start)
        {
            int i = start.Column;
            int lineIndex = start.Row;

        search:
            string line = codeLines[lineIndex].Trim();
            while (i < line.Length - 1 && !(line[i] == '*' && line[i + 1] == '/'))
            {
                i++;
            }

            if (i == line.Length - 1 || line.Length == 1 || line.Length == 0)
            {
                i = 0;
                lineIndex++;
                goto search;
            }
            else
            {
                return new Point(lineIndex, i + 1);
            }
        }

        /// <summary>
        /// Parses a chained variable and returns its components.
        /// </summary>
        /// <param name="index">The start index of the variable.</param>
        /// <param name="programInternalForm">The program internal form.</param>
        /// <returns>The components of the chained variable.</returns>
        protected List<PIFEntry> ParseChainedVariable(int index, List<PIFEntry> programInternalForm)
        {
            List<PIFEntry> vars = new List<PIFEntry>();
            vars.Add(programInternalForm[index]);
            vars.Add(programInternalForm[index + 1]);
            int j = index + 3;
        search:
            while (programInternalForm[j].Symbol == ".")
            {
                if (programInternalForm[j - 1].Symbol != ")")
                {
                    vars.Add(programInternalForm[j - 1]);
                }
                vars.Add(programInternalForm[j]);
                j += 2;
            }

            if (programInternalForm[j].Symbol == "(")
            {
                vars.Add(programInternalForm[j - 1]);
                vars.Add(programInternalForm[j]);
                List<PIFEntry> result = ParseScope(programInternalForm, j, "(", ")");
                if (result.Count == 1)
                {
                    vars.Add(programInternalForm[j + 1]);
                    j += 2;
                }
                else
                {
                    j += result.Count + 1;
                    vars.AddRange(result);
                }
                goto search;
            }
            else
            {
                vars.Add(programInternalForm[j - 1]);
            }

            return vars;
        }

        /// <summary>
        /// Parses a scope section and returns the list of tokens encountered.
        /// </summary>
        /// <param name="programInternalForm">The program internal form.</param>
        /// <param name="start">The start index.</param>
        /// <param name="delimiterOpen">The opening delimiter.</param>
        /// <param name="delimiterClose">The closing delimiter.</param>
        /// <returns>The list of tokens encountered.</returns>
        protected List<PIFEntry> ParseScope(List<PIFEntry> programInternalForm, int start, string delimiterOpen, string delimiterClose)
        {
            int bracketCount = 1;
            int i = start + 1;

            List<PIFEntry> result = new List<PIFEntry>();

            while (bracketCount > 0)
            {
                result.Add(programInternalForm[i]);
                if (programInternalForm[i].Symbol == delimiterOpen)
                {
                    bracketCount++;
                }

                if (programInternalForm[i].Symbol == delimiterClose)
                {
                    bracketCount--;
                    if (bracketCount == 0)
                    {
                        break;
                    }
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Verifies if any errors are related to the current variable.
        /// </summary>
        /// <param name="variable">The variable to inspect.</param>
        /// <param name="programInternalForm">The program internal form.</param>
        /// <param name="index">The index of the variable in the program internal form.</param>
        /// <param name="line">The line where the current pragma appears.</param>
        /// <param name="isLocal">A hashtable that marks which variables are local.</param>
        /// <returns>The error found.</returns>
        protected Error VerifyVariable(string variable, List<PIFEntry> programInternalForm, int index, int line, Dictionary<string, bool> isLocal)
        {
            if ((variable == "Thread" && programInternalForm[index + 1].Code == 0) ||
                    (variable == "Thread" && programInternalForm[index + 1].Symbol == "[" &&
                     programInternalForm[index + 2].Symbol == "]" && programInternalForm[index + 3].Code == 0))
            {
                return new Error(line,
                    "Nested parallelism detected. This may lead to performance degradation.");
            }

            if (variable == "QueueUserWorkItem" &&
                programInternalForm[index - 1].Symbol == "." &&
                programInternalForm[index - 2].Symbol == "ThreadPool")
            {
               return new Error(line,
                    "Nested parallelism detected. This may lead to performance degradation.");
            }

            if (!ScopeScanner.Types.Contains(programInternalForm[index - 1].Symbol))
            {
                if (programInternalForm[index - 1].Code == 0
                    || (programInternalForm[index - 1].Symbol == "]")
                    || (programInternalForm[index - 1].Symbol == ">")
                    || (programInternalForm[index - 1].Symbol == "var")) // this is a local variable.
                {
                    isLocal.Remove(variable);
                    isLocal.Add(variable, true);
                    return null;
                }

                if (index < programInternalForm.Count && programInternalForm[index + 1].Code == 0) // this is a type.
                {
                    return null;
                }
                else
                {
                    if (index < programInternalForm.Count &&
                        (programInternalForm[index + 1].Symbol == "(" || // method call.
                        programInternalForm[index + 1].Symbol == "=>")) // lambda parameter.
                    {
                        return null;
                    }
                    else
                    {
                        bool value = false;
                        isLocal.TryGetValue(variable, out value);
                        if (!value) // global variable detected.
                        {
                            if (index > 0 &&
                                (programInternalForm[index - 1].Symbol == "ref" ||
                                 programInternalForm[index - 1].Symbol == "out")) // variable modified by method call.
                            {
                                if (programInternalForm[index + 1].Symbol == "," ||
                                    programInternalForm[index + 1].Symbol == ")")
                                {
                                    return new Error(line,
                                                  "Variable \'" + variable + "\' may be concurrently modified. Consider using a lock or declaring it locally.");
                                }

                                if (programInternalForm[index + 1].Symbol == ".")
                                {
                                    List<PIFEntry> vars = ParseChainedVariable(index, programInternalForm);
                                    StringBuilder builder = new StringBuilder();
                                    foreach (PIFEntry var in vars)
                                    {
                                        builder.Append(var.Symbol);
                                    }

                                    return new Error(line,
                                                  "Variable \'" + builder.ToString() + "\' may be concurrently modified. Consider using a lock or declaring it locally.");
                                }
                            }

                            if (programInternalForm[index + 1].Symbol == "." &&
                                programInternalForm[index - 1].Symbol != ".")
                            {
                                List<PIFEntry> vars = ParseChainedVariable(index, programInternalForm);
                                StringBuilder builder = new StringBuilder();
                                foreach (PIFEntry var in vars)
                                {
                                    builder.Append(var.Symbol);
                                }

                                if (ScopeScanner.AssignmentOperators.Contains((programInternalForm[index + vars.Count].Symbol)))
                                {
                                    return new Error(line,
                                                  "Variable \'" + builder.ToString() + "\' may be concurrently modified. Consider using a lock or declaring it locally.");
                                }
                                return null;
                            }

                            if (index < programInternalForm.Count &&
                                programInternalForm[index - 1].Symbol != "." &&
                                ScopeScanner.AssignmentOperators.Contains((programInternalForm[index + 1].Symbol)))
                            {
                                return new Error(line,
                                   "Variable \'" + variable + "\' may be concurrently modified. Consider using a lock or declaring it locally.");
                            }
                        }
                    }
                }
            }
            else
            {
                isLocal.Remove(variable);
                isLocal.Add(variable, true);
            }

            return null;
        }
    }
}
