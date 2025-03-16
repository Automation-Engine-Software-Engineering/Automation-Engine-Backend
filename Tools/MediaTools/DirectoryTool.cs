using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MediaTools
{
    public class DirectoryTool
    {
        public static string wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private static string FormPath = Path.Combine("upload", "form");
        public static string FormContentPath = Path.Combine(FormPath,"content", "image");
        public static string FormBackgroundImagePath = Path.Combine(FormPath, "backgroundimage");
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

    }
    public static class DirectoryExtentionTool
    {
        public static string GetFullPath(this string path,params string[] pathBetween) =>
            Path.Combine(DirectoryTool.wwwroot, Path.Combine(pathBetween) , path);
    }
}
