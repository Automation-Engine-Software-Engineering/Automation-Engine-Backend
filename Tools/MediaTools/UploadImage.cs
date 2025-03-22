using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Tools.MediaTools;

namespace Tools
{
    public static class UploadImage
    {
        public static async Task<string?> UploadFormMedia(IFormFile? file)
        {
            if (file == null)
                return null;
            ValidationFile.ValidateImageFile(file,20.ToMB());
            var fileName = await FileTool.UploadFileWithUniqueName(file,DirectoryTool.FormContentPath.GetFullPath());

            return "\\"+ Path.Combine(DirectoryTool.FormContentPath, fileName);
        }
        public static async Task<string?> UploadFormBackgroundImage(IFormFile? file)
        {
            if (file == null)
                return null;
            ValidationFile.ValidateImageFile(file, 20.ToMB());
            var fileName = await FileTool.UploadFileWithUniqueName(file, DirectoryTool.FormBackgroundImagePath.GetFullPath());

            return "\\" + Path.Combine(DirectoryTool.FormBackgroundImagePath, fileName);
        }
    }
}
