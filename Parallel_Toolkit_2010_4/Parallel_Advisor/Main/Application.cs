using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Parallel_Advisor.Main
{
    public static class Application
    {
        public static List<int> PragmaLines { get; set; }
        public static SortedDictionary<int, int> Intervals { get; set; }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Application()
        {
            PragmaLines = new List<int>();
            Intervals = new SortedDictionary<int, int>();
        }
    }
}
