using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Threading;

namespace Thread_Profiler.Threads
{
    public class ThreadProfiler
    {
        private class ThreadData
        {
            public int ThreadID { get; set; }
            public double SampleData { get; set; }
            public int RunIndex { get; set; }

            /// <summary>
            /// Creates a new instance of class ThreadData.
            /// </summary>
            /// <param name="id">The id of the thread.</param>
            /// <param name="data">The sample data collected.</param>
            /// <param name="runIndex">The index of the sample data.</param>
            public ThreadData(int id, double data, int runIndex)
            {
                ThreadID = id;
                SampleData = data;
                RunIndex = runIndex;
            }
        }

        /// <summary>
        /// Samples the execution of a given process and returns per thread cpu usage information.
        /// </summary>
        /// <param name="duration">The duration of the sampling.</param>
        /// <param name="processPath">The executable to run.</param>
        /// <param name="processArgs">The arguments of the executable.</param>
        /// <returns>Per thread cpu usage information.</returns>
        public ThreadInfo[] SampleExecution(int duration, string processPath, string processArgs)
        {
            int interval = 500;
            List<ThreadInfo> result = new List<ThreadInfo>();

            Process process = new Process();
            process.StartInfo.FileName = processPath;
            process.StartInfo.Arguments = processArgs;

            List<ThreadData> globalThreadData = new List<ThreadData>();
            List<int> threadIds = new List<int>();
            int[] runIndexes = new int[65535];

            int runs = 0;
            double totalTime = 0;

            process.Start();

            try
            {
                while (!process.HasExited)
                {
                    Thread.Sleep(interval);
                    runs++;
                    if (runs == duration * 1000 / interval)
                    {
                        process.Kill();
                        break;
                    }

                    process.Refresh();
                    totalTime = process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;

                    foreach (ProcessThread thread in process.Threads)
                    {
                        if (!threadIds.Contains(thread.Id))
                        {
                            threadIds.Add(thread.Id);
                            runIndexes[thread.Id] = 0;
                        }
                        runIndexes[thread.Id]++;

                        globalThreadData.Add(new ThreadData(thread.Id, thread.TotalProcessorTime.TotalMilliseconds, runIndexes[thread.Id]));
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }

            int i = 0;
            foreach (int id in threadIds)
            {
                double minUsage = Double.PositiveInfinity;
                double avgUsage = 0;
                double maxUsage = 0;
                double timePercentage = 0;
                double maxSampleData = 0;

                double prev = 0;
                foreach (ThreadData threadData in globalThreadData)
                {
                    if (threadData.ThreadID == id)
                    {
                        if (threadData.SampleData < (threadData.RunIndex * interval))
                        {
                            if (minUsage > (threadData.SampleData - prev) / interval)
                            {
                                minUsage = (threadData.SampleData - prev) / interval;
                            }

                            if (maxUsage < (threadData.SampleData - prev) / interval)
                            {
                                maxUsage = (threadData.SampleData - prev) / interval;
                            }

                            avgUsage = threadData.SampleData / (threadData.RunIndex * interval);
                        }
                        
                        prev = threadData.SampleData;
                        maxSampleData = threadData.SampleData;
                    }
                }

                if (maxSampleData < totalTime)
                {
                    timePercentage = maxSampleData / totalTime;
                }
                else
                {
                    timePercentage = maxSampleData / (Math.Ceiling(maxSampleData / 1000) * 1000);
                }

                minUsage = minUsage > 1 ? 1 : minUsage;
                maxUsage = maxUsage > 1 ? 1 : maxUsage;
                avgUsage = avgUsage > 1 ? 1 : avgUsage;

                result.Add(new ThreadInfo(i, Math.Round(timePercentage * 100, 2),
                    Math.Round(minUsage * 100, 2), Math.Round(avgUsage * 100, 2), Math.Round(maxUsage * 100, 2)));
                i++;
            }

            return result.ToArray();
        }
    }
}
