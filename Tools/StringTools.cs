using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;

namespace Tools
{
    public static class StringTools
    {
        public static bool IsValidateStringCommand(this string input)
        {
            string pattern = @"^(?![0-9])[a-zA-Z][a-zA-Z0-9]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                throw new CustomException("خظا در نوشتار دستور وجود دارد");
        }

        
        public static bool IsValidateString(this string input)
        {
            string pattern = @"(?<![-_])([a-zA-Z\u0600-\u06FF]+(?:[ _-][a-zA-Z\u0600-\u06FF]+)*)?(?![-_])";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                throw new CustomException("خظا در نوشتار وجود دارد");
        }
    }
}
