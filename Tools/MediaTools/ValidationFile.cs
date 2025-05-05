using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
namespace Tools.MediaTools
{
    public class ValidationFile
    {
        private static readonly string[] ValidExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };

        public static bool ValidateImageFile(IFormFile file, long maxFileSizeInBytes)
        {
            // 1. بررسی پسوند فایل
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ValidExtensions.Contains(fileExtension))
                throw new CustomException("File", "FileType", maxFileSizeInBytes);

            // 2. بررسی حجم فایل
            if (file.Length > maxFileSizeInBytes)
                throw new CustomException("File", "FileSize", maxFileSizeInBytes);

            // 3. بررسی ساختار داخلی فایل برای تایید عکس بودن
            if (!IsValidImage(file))
                throw new CustomException("File", "CorruptedFile", maxFileSizeInBytes);

            return true;
        }

        private static bool IsValidImage(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using var image = Image.FromStream(stream);
                    return true;
                }
            }
            catch
            {
                return false; 
            }
        }
    }
    public static class ValidationFileExtention
    {
        public static long ToMB(this int megabyte) => megabyte * 1024 * 1024;
    }
}
