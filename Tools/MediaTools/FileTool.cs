using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MediaTools
{
    internal class FileTool
    {
        internal static async Task<string> UploadFileWithUniqueName(IFormFile file,string uploadDirectoryPath)
        {
            if (file == null)
                throw new Exception("file is empty");

            DirectoryTool.CreateDirectory(uploadDirectoryPath);

            string fileKey = Guid.NewGuid().ToString().Replace("-", "");
            string fileName = fileKey + Path.GetExtension(file.FileName).ToLower();
            string filePath = Path.Combine(uploadDirectoryPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
