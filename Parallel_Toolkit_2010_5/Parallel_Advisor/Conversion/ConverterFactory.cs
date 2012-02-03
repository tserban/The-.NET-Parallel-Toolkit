using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Conversion
{
    public class ConverterFactory : IConverterFactory
    {
        /// <summary>
        /// Creates a new code converter of the given type that works on the given file.
        /// </summary>
        /// <param name="type">The type of converter.</param>
        /// <param name="inputFile">The input file on which to work.</param>
        /// <returns>A new code converter.</returns>
        public IConverter CreateConverter(string type, string inputFile)
        {
            switch (type)
            {
                case "for":
                    return new ParallelForConverter(inputFile);
                case "while":
                    return new ParallelWhileConverter(inputFile);
                case "tasks":
                    return new ParallelTasksConverter(inputFile);
                case "lock":
                    return new ParallelLockConverter(inputFile);
                case "atomic":
                    return new ParallelAtomicConverter(inputFile);
                case "barrier":
                    return new ParallelBarrierConverter(inputFile);
                default:
                    return null;
            }
        }
    }
}
