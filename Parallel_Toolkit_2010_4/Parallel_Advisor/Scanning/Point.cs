using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Scanning
{
    public class Point
    {
        public int Row { get; set; }
        public int Column { get; set; }

        /// <summary>
        /// Creates a new instance of class Point.
        /// </summary>
        public Point()
        {
            Row = 0;
            Column = 0;
        }

        /// <summary>
        /// Creates a new instance of class Point.
        /// </summary>
        /// <param name="row">The matrix row.</param>
        /// <param name="column">The matrix column.</param>
        public Point(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
