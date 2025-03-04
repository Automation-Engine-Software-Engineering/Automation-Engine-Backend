using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;

namespace Tools
{
    public static class StringTools
    {
        public static bool IsValidateString(this string input)
        {
            string pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                throw new CostumExeption("خظا در نوشتار وجود دارد");
        }
    }
}
