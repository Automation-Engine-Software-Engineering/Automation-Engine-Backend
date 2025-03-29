using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.TextTools
{
    public static class CustomizeString
    {
        public static string InsertSpaces(this string input)
        {
            var result = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsUpper(c) && result.Length > 0)
                {
                    result.Append(' ');
                }
                result.Append(c);
            }
            return result.ToString();
        }
        public static bool IsNullOrEmpty(this string? input) => string.IsNullOrEmpty(input);
        public static bool IsNullOrWhiteSpace(this string? input) => string.IsNullOrWhiteSpace(input);
    }
}
