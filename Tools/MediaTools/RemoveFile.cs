using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace Tools
{
    internal static class RemoveFile
    {
        internal static bool Remove(string path)
        {
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }
    }
}
