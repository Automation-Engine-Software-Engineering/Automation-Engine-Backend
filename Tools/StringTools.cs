using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;

namespace Tools
{
    public static class StringTools
    {
        public static bool IsValidateStringCommand(this string input)
        {
            string pattern = @"^(?![0-9])[\u0600-\u06FFa-zA-Z][\u0600-\u06FFa-zA-Z0-9 ()]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException<string>(new ValidationDto<string>(false, "Defult", "CorruptedStringCommand", input), 500);
        }

        public static bool IsValidateStringQuery(this string input)
        {
            string pattern = @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|EXEC|CREATE|TRUNCATE|REPLACE|LOAD_FILE|INTO OUTFILE|OR\s+1=1|--|;|\/\*|\*\/)\b";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException<string>(new ValidationDto<string>(false, "Defult", "CorruptedStringQuery", input), 500);
        }


        public static bool IsValidateString(this string input)
        {
            string pattern = @"(?<![-_])([a-zA-Z\u0600-\u06FF]+(?:[ _-][a-zA-Z\u0600-\u06FF]+)*)?(?![-_])";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException<string>(new ValidationDto<string>(false, "Defult", "CorruptedString", input), 500);
        }

    }
}
