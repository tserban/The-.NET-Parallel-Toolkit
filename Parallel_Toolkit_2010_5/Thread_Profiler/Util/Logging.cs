using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Thread_Profiler.Util
{
    public static class Logging
    {
        private static string outputFile = "output.txt";

        /// <summary>
        /// Logs a message to the output file.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
        {
            TextWriter writer = new StreamWriter(outputFile, true);
            writer.WriteLine(DateTime.Now.ToString());
            writer.WriteLine(message);
            writer.WriteLine();
            writer.Close();
        }
    }
}
