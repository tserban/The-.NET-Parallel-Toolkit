using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Thread_Profiler.Threads
{
    public static class ProcessorInformation
    {
        public static string Name { get; private set; }
        public static int Cores { get; private set; }
        public static int LogicalProcessors { get; private set; }
        public static int PhysicalProcessors { get; private set; }

        static ProcessorInformation()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name, NumberOfCores, NumberOfLogicalProcessors from Win32_Processor");
            foreach (var item in searcher.Get())
            {
                Name = item["Name"].ToString();
                Cores = Int32.Parse(item["NumberOfCores"].ToString());
                LogicalProcessors = Int32.Parse(item["NumberOfLogicalProcessors"].ToString());
                break;
            }

            searcher = new ManagementObjectSearcher("select NumberOfProcessors from Win32_ComputerSystem");
            foreach (var item in searcher.Get())
            {
                PhysicalProcessors = Int32.Parse(item["NumberOfProcessors"].ToString());
                break;
            }
        }
    }
}
