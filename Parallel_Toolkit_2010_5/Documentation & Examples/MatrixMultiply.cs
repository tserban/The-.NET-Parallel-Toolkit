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
        static int[][] matrix1;
        static int[][] matrix2;
        static int[][] result;
        private static int size1 = 1024;
        private static int size2 = 1024;
        private static int size3 = 1024;

        public static void Multiply()
        {
            //@ parallel for dynamic
            for (int i = 0; i < size1; i++)
            {
                for (int j = 0; j < size3; j++)
                {
                    int partial = 0;
                    for (int k = 0; k < size2; k++)
                    {
                        partial += matrix1[i][k] * matrix2[k][j];
                    }
                    result[i][j] += partial;
                }
            }
        }
        
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int i, j;
            matrix1 = new int[size1][];
            for (i = 0; i < size1; i++)
            {
                matrix1[i] = new int[size2];
            }

            for (i = 0; i < size1; i++)
            {
                for (j = 0; j < size2; j++)
                {
                    matrix1[i][j] = 2;
                }
            }

            matrix2 = new int[size2][];
            for (i = 0; i < size2; i++)
            {
                matrix2[i] = new int[size3];
            }

            for (i = 0; i < size2; i++)
            {
                for (j = 0; j < size3; j++)
                {
                    matrix2[i][j] = 2;
                }
            }

            result = new int[size1][];
            for (i = 0; i < size1; i++)
            {
                result[i] = new int[size3];
            }

            DateTime start = DateTime.Now;
            Multiply();
            DateTime end = DateTime.Now;
            Console.WriteLine((end - start).TotalMilliseconds);

            for (i = 0; i < size1; i++)
            {
                for (j = 0; j < size3; j++)
                {
                    if (result[i][j] != 4096)
                    {
                        Console.WriteLine("Wrong!");
                        return;
                    }
                }
            }
        }
    }
}
