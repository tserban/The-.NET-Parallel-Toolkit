using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;

namespace Parallel_Advisor.Scanning
{
    public class ScopeScanner
    {
        public static string[] Operators = { "<<=", ">>=", "&&", "||", "++", "--", "<<", ">>", "==", "!=", "<=",
                                       ">=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "=>", ".",
                                       "=", "+", "-", "*", "/", "%", "&", "|", "^", "!", "~", "?", ":", "<", ">" };

        public static string[] Keywords = { "abstract", "event", "new", "struct", "as", "explicit", "null", "switch",
                                      "base", "extern", "object", "this", "bool", "false", "operator", "throw",
                                      "break", "finally", "out", "true", "byte", "fixed", "override", "try",
                                      "case", "float", "params", "typeof", "catch", "for", "private", "uint",
                                      "char", "foreach", "protected", "ulong", "checked", "goto", "public", "unchecked",
                                      "class", "if", "readonly", "unsafe", "const", "implicit", "ref", "ushort",
                                      "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual",
                                      "default", "interface", "sealed", "volatile", "delegate", "internal", "short", "void",
                                      "do", "is", "sizeof", "while", "double", "lock", "stackalloc", "else", "long", "static",
                                      "enum", "namespace", "string" };

        public static string[] InstructionSeparators = { ",", ";", "[", "]", "(", ")" };
        public static string[] ScopeSeparators = { "{", "}" };
        public static string[] Space = { " " };
        public static string[] Types = { "object", "bool", "byte", "float", "char", "ulong", "ushort", 
                                         "decimal", "int", "sbyte", "short", "double", "long", "string" };
        public static string[] AssignmentOperators = { "<<=", ">>=", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", 
                                                       "|=", "^=", "=" };
        public static string[] ComparisonOperators = { "==", "!=", "<=", ">=", "<", ">" };
        public static string[] LogicalOperators = { "&&", "||", "!", "&", "|" };
        
        private Dictionary<string, int> mapping = new Dictionary<string, int>();
        public List<PIFEntry> ProgramInternalForm { get; set; }

        /// <summary>
        /// Creates a new instance of class ScopeScanner.
        /// </summary>
        public ScopeScanner()
        {
            ProgramInternalForm = new List<PIFEntry>();

            int i = 2;
            foreach (string op in Operators)
            {
                mapping.Add(op, i++);
            }

            foreach (string keyword in Keywords)
            {
                mapping.Add(keyword, i++);
            }

            foreach (string separator in InstructionSeparators)
            {
                mapping.Add(separator, i++);
            }

            foreach (string separator in ScopeSeparators)
            {
                mapping.Add(separator, i++);
            }
        }

        /// <summary>
        /// Parses a string and fills the ProgramInternalForm.
        /// </summary>
        /// <param name="parseString">The string to parse.</param>
        public void ParseString(string parseString)
        {
            ProgramInternalForm.Clear();

            int i = 0;
            while (i < parseString.Length && ScopeSeparators.Contains(parseString[i].ToString()))
            {
                int code = 0;
                mapping.TryGetValue(parseString[i].ToString(), out code);
                ProgramInternalForm.Add(new PIFEntry(code, parseString[i].ToString()));
                i++;
            }

            string[] scopes = parseString.Split(ScopeSeparators, StringSplitOptions.None);
            foreach (string scope in scopes)
            {
                string[] instructions = scope.Split(InstructionSeparators, StringSplitOptions.None);
                foreach (string instruction in instructions)
                {
                    string[] identifiers = instruction.Split(Operators, StringSplitOptions.None);

                    foreach (string identifier in identifiers)
                    {
                        string[] tokens = identifier.Split(Space, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string token in tokens)
                        {
                            string trimmedToken = token.Trim();
                            if (Keywords.Contains(trimmedToken))
                            {
                                int code = 0;
                                mapping.TryGetValue(trimmedToken, out code);
                                ProgramInternalForm.Add(new PIFEntry(code, trimmedToken));
                            }
                            else
                            {
                                if (IsConstant(trimmedToken))
                                {
                                    ProgramInternalForm.Add(new PIFEntry(1, trimmedToken));
                                }
                                else
                                {
                                    ProgramInternalForm.Add(new PIFEntry(0, trimmedToken));
                                }
                            }

                            i += token.Length;
                            while (i < parseString.Length && parseString[i] == ' ')
                            {
                                i++;
                            }
                        }

                        while (i < parseString.Length - 2 && Operators.Contains(parseString.Substring(i, 3)))
                        {
                            int code = 0;
                            mapping.TryGetValue(parseString.Substring(i, 3).ToString(), out code);
                            ProgramInternalForm.Add(new PIFEntry(code, parseString.Substring(i, 3)));
                            i += 3;
                        }

                        while (i < parseString.Length - 1 && Operators.Contains(parseString.Substring(i, 2)))
                        {
                            int code = 0;
                            mapping.TryGetValue(parseString.Substring(i, 2), out code);
                            ProgramInternalForm.Add(new PIFEntry(code, parseString.Substring(i, 2)));
                            i += 2;
                        }

                        while (i < parseString.Length && Operators.Contains(parseString[i].ToString()))
                        {
                            int code = 0;
                            mapping.TryGetValue(parseString[i].ToString(), out code);
                            ProgramInternalForm.Add(new PIFEntry(code, parseString[i].ToString()));
                            i++;
                        }

                        while (i < parseString.Length && parseString[i] == ' ')
                        {
                            i++;
                        }
                    }

                    while (i < parseString.Length && InstructionSeparators.Contains(parseString[i].ToString()))
                    {
                        int code = 0;
                        mapping.TryGetValue(parseString[i].ToString(), out code);
                        ProgramInternalForm.Add(new PIFEntry(code, parseString[i].ToString()));
                        i++;
                    }

                    while (i < parseString.Length && parseString[i] == ' ')
                    {
                        i++;
                    }
                }

                while (i < parseString.Length && ScopeSeparators.Contains(parseString[i].ToString()))
                {
                    int code = 0;
                    mapping.TryGetValue(parseString[i].ToString(), out code);
                    ProgramInternalForm.Add(new PIFEntry(code, parseString[i].ToString()));
                    i++;
                }

                while (i < parseString.Length && parseString[i] == ' ')
                {
                    i++;
                }
            }
        }
        
        /// <summary>
        /// Checks whether a token is a constant value.
        /// </summary>
        /// <param name="token">The given token.</param>
        /// <returns>True if the token is a constant, false otherwise.</returns>
        private bool IsConstant(string token)
        {
            double result = 0;
            if (Double.TryParse(token, out result)) // numeric value.
            {
                return true;
            }

            if (token[0] == '\"' && token[token.Length - 1] == '\"') // string.
            {
                return true;
            }

            if (token.Length == 3 && token[0] == '\'' && token[2] == '\'') // char.
            {
                return true;
            }

            return false;
        }
    }
}
