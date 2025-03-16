using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;

namespace Tools.TextTools
{
    public static class ValidationString
    {
        public static bool IsValidStringCommand(this string input)
        {
            string pattern = @"^(?![0-9])[a-zA-Z][a-zA-Z0-9]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException<string>(new ValidationDto<string>(false, "Defult", "CorruptedStringCommand", input), 500);
        }


        public static bool IsValidString(this string input)
        {
            string pattern = @"(?<![-_])([a-zA-Z\u0600-\u06FF]+(?:[ _-][a-zA-Z\u0600-\u06FF]+)*)?(?![-_])";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                 throw new CustomException<string>(new ValidationDto<string>(false, "Defult", "CorruptedString", input), 500);
        }

    }
}
