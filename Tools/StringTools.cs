using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;

namespace Tools
{
    public static class StringTools
    {
        public static bool IsValidateString(this string input)
        {
            string pattern = @"^[A-Za-z_آ-ی][A-Za-z0-9_آ-ی()]*$";
            return Regex.IsMatch(input, pattern) ? Regex.IsMatch(input, pattern) :
                throw new CustomExeption("خطا در نوشتار وجود دارد");
        }

    }
}
