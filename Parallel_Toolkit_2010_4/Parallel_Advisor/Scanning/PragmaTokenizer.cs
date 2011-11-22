using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Scanning
{
    public class PragmaTokenizer
    {
        public static string[] keywords = { "//@", "parallel", "for", "static", "dynamic", "while", "tasks", "task", 
                                            "nosync", "pooled", "lock", "atomic", "membarrier" };
        private static Dictionary<string, int> mapping = null;

        static PragmaTokenizer()
        {
            mapping = new Dictionary<string, int>();
            int i = 2;
            foreach (string keyword in keywords)
            {
                mapping.Add(keyword, i);
                i++;
            }
        }

        /// <summary>
        /// Splits a pragma into tokens.
        /// </summary>
        /// <param name="pragma">The pragma to split.</param>
        /// <returns>The PIF representation of the tokenized pragma.</returns>
        public static List<PIFEntry> TokenizePragma(string pragma)
        {
            List<PIFEntry> result = new List<PIFEntry>();
            string[] space = { " " };
            string[] tokens = pragma.Split(space, StringSplitOptions.RemoveEmptyEntries);
            foreach(string token in tokens)
            {
                if (keywords.Contains(token))
                {
                    int code = 0;
                    mapping.TryGetValue(token, out code);
                    result.Add(new PIFEntry(code, token));
                }
                else
                {
                    if (IsConstant(token))
                    {
                        result.Add(new PIFEntry(1, token));
                    }
                    else
                    {
                        result.Add(new PIFEntry(0, token));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Verifies if a token represents an integer constant.
        /// </summary>
        /// <param name="token">The token to inspect.</param>
        /// <returns>True if the token is a numeric constant, false otherwise.</returns>
        private static bool IsConstant(string token)
        {
            long result = 0;
            if (Int64.TryParse(token, out result) && result >= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Splits a pragma into tokens.
        /// </summary>
        /// <param name="pragma">The pragma to split.</param>
        /// <returns>The list of tokens in the pragma.</returns>
        public static List<string> ReturnParseSequence(string pragma)
        {
            List<string> result = new List<string>();
            List<PIFEntry> tokens = TokenizePragma(pragma);
            foreach(PIFEntry token in tokens)
            {
                if (token.Code <= 1)
                {
                    result.Add(token.Code.ToString());
                }
                else
                {
                    result.Add(token.Symbol);
                }
            }
            return result;
        }
    }
}
