using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Scanning
{
    /// <summary>
    /// Represents an entry in the program internal form.
    /// </summary>
    public class PIFEntry
    {
        /// <summary>
        /// The code of the symbol in the program internal form.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// The actual name of the symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Creates a new instance of class PIFEntry.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="symbol"></param>
        public PIFEntry(int code, string symbol)
        {
            Code = code;
            Symbol = symbol;
        }
    }
}
