using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Parallel_Advisor.Util
{
    public static class UtilOperations
    {
        /// <summary>
        /// Returns the number of newlines in the given string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>The number of newlines in the given string.</returns>
        public static int CountLinesInString(string inputString)
        {
            return Regex.Matches(inputString, Environment.NewLine).Count;
        }
    }
}
