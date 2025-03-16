using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Tools.MediaTools;

namespace Tools
{
    public static class RemoveFile
    {
        public static bool Remove(string? reletivePath)
        {
            if (string.IsNullOrWhiteSpace(reletivePath))
                return false;
            var path = reletivePath.GetFullPath();
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }
    }
}
