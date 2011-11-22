using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Parsing
{
    /// <summary>
    /// Represents a production in a grammar.
    /// </summary>
    public class Production
    {
        /// <summary>
        /// The left side of the production.
        /// </summary>
        public string LeftSide { get; set; }

        /// <summary>
        /// The right side of the production.
        /// </summary>
        public List<string> RightSide { get; set; }

        /// <summary>
        /// Creates a new instance of class Production.
        /// </summary>
        /// <param name="leftSide">The left side of the production.</param>
        /// <param name="rightSide">The right side of the production</param>
        public Production(string leftSide, List<string> rightSide)
        {
            LeftSide = leftSide;
            RightSide = rightSide;
        }

        public override string ToString()
        {
            string right = null;
            foreach (string str in this.RightSide)
            {
                right += str + " ";
            }
            return LeftSide + "->" + right.Trim();
        }

        public override bool Equals(object obj)
        {
            Production production = obj as Production;

            bool equal = true;
            for (int i = 0; i < production.RightSide.Count; i++)
            {
                if (production.RightSide[i] != this.RightSide[i])
                {
                    equal = false;
                }
            }

            if (this.LeftSide == production.LeftSide && equal)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
