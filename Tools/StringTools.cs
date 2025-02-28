using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    public static class StringTools
    {
        public static bool IsValidateString(this string input)
        {
            string pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                throw new Exception("the input not define");
        }
    }
}
