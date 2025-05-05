using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;

namespace Tools.TextTools
{
    public static class ValidationString
    {
        public static bool IsValidStringCommand(this string input)
        {
            string pattern = @"^(?![0-9])[\u0600-\u06FFa-zA-Z][\u0600-\u06FFa-zA-Z0-9 ()]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException("Default", "CorruptedStringCommand", input);
        }

        public static bool IsValidateStringQuery(this string input)
        {
            string pattern = @"\b(?!(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|EXEC|CREATE|TRUNCATE|REPLACE|LOAD_FILE|INTO OUTFILE|OR\s+1=1|--|;|\/\*|\*\/)\b).+\b";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException("Default", "CorruptedStringQuery", input);
        }




        public static bool IsValidString(this string? input)
        {
            if (input == null)
                return true;
            string pattern = @"(?<![-_])([a-zA-Z\u0600-\u06FF]+(?:[ _-][a-zA-Z\u0600-\u06FF]+)*)?(?![-_])";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException("Default", "CorruptedString", input);
        }
    }
}
