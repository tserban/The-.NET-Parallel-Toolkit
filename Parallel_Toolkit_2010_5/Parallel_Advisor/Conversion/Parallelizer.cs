using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Parallel_Advisor.Conversion
{
    public class Parallelizer
    {
        /// <summary>
        /// Reads an XML file containing parallel construct types and returns their identifiers as strings.
        /// </summary>
        /// <param name="file">The XML file.</param>
        /// <returns>The identifiers of the parallel constructs.</returns>
        private static string[] ReadTypes(string file)
        {
            List<string> result = new List<string>();
            XmlDocument document = new XmlDocument();
            document.Load(file);
            XmlNodeList typeList = document.SelectNodes("//type");

            foreach (XmlNode node in typeList)
            {
                result.Add(node.InnerText);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Applies all conversions to the initial file and produces the final file.
        /// </summary>
        /// <param name="initialFile">The initial file.</param>
        /// <param name="finalFile">The final file.</param>
        public static void ApplyConversions(string initialFile, string finalFile)
        {
            ConverterFactory factory = new ConverterFactory();
            bool success = false;
            string inputFile = initialFile;
            foreach(string type in ReadTypes("types.xml"))
            {
                success = factory.CreateConverter(type, inputFile).ScanAndConvert(finalFile);
                if (success)
                {
                    inputFile = finalFile;
                }
            }
        }
    }
}
