using System;
using System.Collections.Generic;
using System.Text;

namespace LegendaryMapper
{
    public static class CSVParser
    {
        public static string[] Parse(string csv, char delimiter)
        {
            List<string> strings = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            bool inString = false;

            for (int i = 0; i < csv.Length; i++)
            {
                if (csv[i] == delimiter && !inString)
                {
                    strings.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
                else if (csv[i] == '"')
                {
                    inString = !inString;
                }
                else
                {
                    stringBuilder.Append(csv[i]);
                }
            }

            strings.Add(stringBuilder.ToString());
            return strings.ToArray();
        }
    }
}
