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
                throw new CustomException<long>(new ValidationDto<long>(false, "File", "FileType", maxFileSizeInBytes), 500);

            // 2. بررسی حجم فایل
            if (file.Length > maxFileSizeInBytes)
                throw new CustomException<long>(new ValidationDto<long>(false, "File", "FileSize", maxFileSizeInBytes), 500);

            // 3. بررسی ساختار داخلی فایل برای تایید عکس بودن
            if (!IsValidImage(file))
                throw new CustomException<long>(new ValidationDto<long>(false, "File", "CorruptedFile", maxFileSizeInBytes), 500);

            return true;
        }

        private static bool IsValidImage(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    // استفاده از System.Drawing برای بررسی نوع تصویر
                    using var image = Image.FromStream(stream);
                    return true; // اگر موفق شد تصویر را بخواند، فایل معتبر است
                }
            }
            catch
            {
                return false; // هرگونه خطا نشان‌دهنده فایل نامعتبر است
            }
        }
        
    }
    public static class ValidationFileExtention
    {
        public static long ToMB(this int megabyte) => megabyte * 1024 * 1024;
    }
}
