using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Conversion
{
    public interface IConverterFactory
    {
        /// <summary>
        /// Creates a new code converter of the given type that works on the given file.
        /// </summary>
        /// <param name="type">The type of converter.</param>
        /// <param name="inputFile">The input file on which to work.</param>
        /// <returns>A new code converter.</returns>
        IConverter CreateConverter(string type, string inputFile);
    }
}
