using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static int[] inputArray;
        static int[] tempArray;
        static int cpus = Environment.ProcessorCount;

        static void merge(int lo, int m, int hi)
        {
            int i, j, k;

            for (i = lo; i <= hi; i++)
            {
                tempArray[i] = inputArray[i];
            }

            i = lo;
            j = m + 1;
            k = lo;

            while (i <= m && j <= hi)
            {
                if (tempArray[i] < tempArray[j])
                {
                    inputArray[k++] = tempArray[i++];
                }
                else
                {
                    inputArray[k++] = tempArray[j++];
                }
            }

            while (i <= m)
            {
                inputArray[k++] = tempArray[i++];
            }
        }

        static void mergesort(int lo, int hi)
        {
            if (lo < hi)
            {
                int m = (lo + hi) / 2;
                mergesort(lo, m);
                mergesort(m + 1, hi);
                merge(lo, m, hi);
            }
        }

        static void parallel_mergesort(int lo, int hi, int level)
        {
            if (lo < hi)
            {
                int m = (lo + hi) / 2;
                if (Math.Pow(2, level) <= cpus)
                {
                    //@ parallel tasks pooled

                    //@ parallel task
                    {
                        parallel_mergesort(lo, m, level + 1);
                    }

                    //@ parallel task
                    {
                        parallel_mergesort(m + 1, hi, level + 1);
                    }

                    //@ end tasks
                }
                else
                {
                    mergesort(lo, m);
                    mergesort(m + 1, hi);
                }
                merge(lo, m, hi);
            }
        }

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int i = 0;
            int size = 50000000;
            inputArray = new int[size + 1];
            tempArray = new int[size + 1];

            for (i = 1; i <= size; i++)
            {
                inputArray[i] = size - i + 1;
            }

            DateTime start = DateTime.Now;            
            parallel_mergesort(1, size, 1);            
            DateTime end = DateTime.Now;
            Console.WriteLine((end - start).TotalMilliseconds);

            for (i = 1; i <= size; i++)
            {
                if (inputArray[i] != i)
                {
                    Console.WriteLine("Wrong!");
                    return;
                }
            }
        }
    }
}
