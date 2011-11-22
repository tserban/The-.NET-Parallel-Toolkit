using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Errors
{
    public class Error : IComparable
    {
        public int Line { get; set; }
        public string Cause { get; set; }
        public WarningLevels Level { get; private set; }
        public string IconPath { get; set; }

        /// <summary>
        /// Creates a new instance of class Error.
        /// </summary>
        /// <param name="cause">The cause of the error.</param>
        public Error(string cause)
        {
            this.Line = 0;
            this.Cause = cause;
            this.Level = WarningLevels.NonFatal;
            IconPath = "Images\\warning_sign.png";
        }

        /// <summary>
        /// Creates a new instance of class Error.
        /// </summary>
        /// <param name="line">The line at which the error occured.</param>
        /// <param name="cause">The cause of the error.</param>
        public Error(int line, string cause)
            : this(cause)
        {
            Line = line;
        }

        /// <summary>
        /// Creates a new instance of class Error.
        /// </summary>
        /// <param name="cause">The cause of the error.</param>
        /// <param name="level">The level of the error.</param>
        public Error(string cause, WarningLevels level)
        {
            this.Line = 0;
            this.Cause = cause;
            this.Level = level;
            switch (level)
            {
                case WarningLevels.Fatal:
                    IconPath = "Images\\error_sign.png";
                    break;
                case WarningLevels.NonFatal:
                    IconPath = "Images\\warning_sign.png";
                    break;
            }
        }

        /// <summary>
        /// Creates a new instance of class Error.
        /// </summary>
        /// <param name="line">The line at which the error occured.</param>
        /// <param name="cause">The cause of the error.</param>
        /// <param name="level">The level of the error.</param>
        public Error(int line, string cause, WarningLevels level)
            : this(cause, level)
        {
            this.Line = line;
        }

        public override string ToString()
        {
            return "Line " + Line + ": " + Cause + "\n";
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Error error = obj as Error;
            if (this.Line > error.Line)
            {
                return 1;
            }

            if (this.Line < error.Line)
            {
                return -1;
            }

            return 0;
        }

        #endregion
    }
}
