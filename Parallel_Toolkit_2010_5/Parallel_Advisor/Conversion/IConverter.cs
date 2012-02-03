using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Conversion
{
    public interface IConverter
    {
        /// <summary>
        /// Scans the given source code and converts it into parallel code.
        /// </summary>
        /// <param name="outputFile">The file that contains the converted code.</param>
        bool ScanAndConvert(string outputFile);
    }
}
