using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Parsing
{
    /// <summary>
    /// Represents the possible states of a configuration.
    /// </summary>
    public enum States : int
    {
        Normal,
        Back,
        Terminal,
        Error
    }

    /// <summary>
    /// Represents a parsing configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The current state of the configuration.
        /// </summary>
        public States State { get; set; }

        /// <summary>
        /// The current index of the configuration.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The stack containing the sequence already constructed.
        /// </summary>
        public Stack<string> ConstructedStack { get; set; }

        /// <summary>
        /// The stack containing the sequence to be constructed.
        /// </summary>
        public Stack<string> ToBeConstructedStack { get; set; }

        /// <summary>
        /// Creates a new instance of class Configuration.
        /// </summary>
        public Configuration()
        {
            State = States.Normal;
            Index = 0;
            ConstructedStack = new Stack<string>();
            ToBeConstructedStack = new Stack<string>();
        }
    }
}
