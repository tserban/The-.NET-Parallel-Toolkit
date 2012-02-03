using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thread_Profiler.Threads
{
    public class ThreadInfo
    {
        public int ThreadId { get; private set; }
        public double TimePercentage { get; private set; }
        public double MinUsage { get; private set; }
        public double AvgUsage { get; private set; }
        public double MaxUsage { get; private set; }

        /// <summary>
        /// Creates a new instance of class ThreadInfo.
        /// </summary>
        /// <param name="threadId">The id of the thread.</param>
        /// <param name="timePercentage">The percentage of time this thread spent running.</param>
        /// <param name="minUsage">The minimum cpu usage.</param>
        /// <param name="avgUsage">The average cpu usage.</param>
        /// <param name="maxUsage">The maximum cpu usage.</param>
        public ThreadInfo(int threadId, double timePercentage, double minUsage, double avgUsage, double maxUsage)
        {
            ThreadId = threadId;
            TimePercentage = timePercentage;
            MinUsage = minUsage;
            AvgUsage = avgUsage;
            MaxUsage = maxUsage;
        }
    }
}
