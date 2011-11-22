using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Parallel_Advisor.Parsing
{
    /// <summary>
    /// Represents a grammar.
    /// </summary>
    public class Grammar
    {
        /// <summary>
        /// The set of non-terminals.
        /// </summary>
        public List<string> Nonterminals { get; set; }

        /// <summary>
        /// The set of terminals.
        /// </summary>
        public List<string> Terminals { get; set; }

        /// <summary>
        /// The set of productions.
        /// </summary>
        public List<Production> Productions { get; set; }

        /// <summary>
        /// The starting symbol.
        /// </summary>
        public string StartingSymbol { get; set; }

        /// <summary>
        /// Creates a new instance of class Grammar.
        /// </summary>
        public Grammar()
        {
            Productions = new List<Production>();
        }

        /// <summary>
        /// Reads a grammar from a given text file.
        /// </summary>
        /// <param name="file">The path to the text file.</param>
        public void ReadGrammarFromFile(string file)
        {
            TextReader rdr = new StreamReader(file);
            Nonterminals = rdr.ReadLine().Split(' ').ToList();
            Terminals = rdr.ReadLine().Split(' ').ToList();
            string[] arrowSplit = { "->" };

            int count = Int32.Parse(rdr.ReadLine());
            for (int i = 0; i < count; i++)
            {
                string[] split = rdr.ReadLine().Split(arrowSplit, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in split[1].Split('|'))
                {
                    Productions.Add(new Production(split[0], str.Split(' ').ToList()));
                }
            }
            StartingSymbol = rdr.ReadLine();
            rdr.Close();
        }

        /// <summary>
        /// Returns the list of productions for a given symbol.
        /// </summary>
        /// <param name="symbol">The given symbol.</param>
        /// <returns>The list of productions for the given symbol.</returns>
        public List<Production> GetProductionsForSymbol(string symbol)
        {
            List<Production> result = new List<Production>();
            foreach (Production production in Productions)
            {
                if (production.LeftSide == symbol)
                {
                    result.Add(production);
                }
            }

            if (result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
