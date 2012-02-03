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
    public class BarrierPragmaScanner : PragmaScanner
    {
        protected static string parallelBarrier = "//@ parallel membarrier";        
        protected static string grammarFile = @"Grammars\parallel_membarrier_pragma_grammar.txt";

        /// <summary>
        /// Creates a new instance of class BarrierPragmaScanner.
        /// </summary>
        /// <param name="file">The source file to scan.</param>
        public BarrierPragmaScanner(string file)
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
                        break;
                    case ErrorTypes.Valid:
                        Main.Application.PragmaLines.Add(currentLine - 1);
                        break;
                    case ErrorTypes.Missing:
                        break;
                }

                currentIndex++;
            }

            return errorList.ToArray<Error>();
        }

        /// <summary>
        /// Checks if a line contains a valid parallel barrier pragma.
        /// </summary>
        /// <param name="line">The line of code to analyze.</param>
        /// <returns>The return message.</returns>
        protected ErrorTypes CheckForPragmas(string line)
        {
            string trimmedLine = line.Trim();

            List<string> pragmaSplit = PragmaTokenizer.ReturnParseSequence(trimmedLine);
            if (trimmedLine.StartsWith(parallelBarrier))
            {
                if (parser.ValidateExpression(pragmaSplit, pragmaGrammar))
                {
                    return ErrorTypes.Valid;
                }
                return ErrorTypes.Invalid;
            }

            return ErrorTypes.Missing;
        }
    }
}
