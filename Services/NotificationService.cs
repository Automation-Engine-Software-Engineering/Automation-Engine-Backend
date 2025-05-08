using System.Text;
using System.Text.RegularExpressions;
using DataLayer.DbContext;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tools.TextTools;
using ViewModels.ViewModels.Entity;
using ViewModels.ViewModels.FormBuilder;

namespace Services
{
    public interface INotificationService
    {
        Task<List<string>> GetallNotification(int RoleId);
    }

    public class NotificationService : INotificationService
    {
        private readonly DataLayer.DbContext.Context _context;
        private readonly DynamicDbContext _dynamicDbContext;
        private readonly IHtmlService _htmlService;
        public NotificationService(DataLayer.DbContext.Context context, DynamicDbContext dynamicDbContext, IHtmlService htmlService)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
            _htmlService = htmlService;
        }

        public async Task<List<string>> GetallNotification(int RoleId)
        {
            var result = new List<string>();
            var query1 = "select COUNT(*) from [dbo].[DraftContract]";
            var query2 = "select COUNT(*) from [dbo].[ExternalContract]";
            var query3 = "select COUNT(*) from [dbo].[InternalContract]";

            var data1 = await _dynamicDbContext.ExecuteReaderAsync(query1);
            var data2 = await _dynamicDbContext.ExecuteReaderAsync(query2);
            var data3 = await _dynamicDbContext.ExecuteReaderAsync(query3);

            foreach (var item in data1.Data)
            {
                result.Add("عنصر جدی به پیش نویس‌ها اضافه شده است");
            }

              foreach (var item in data2.Data)
            {
                result.Add("عنصر جدی به قرارداد برون دانشگاهی اضافه شده است");
            }

              foreach (var item in data2.Data)
            {
                result.Add("عنصر جدی به درون دانشگاهی اضافه شده است");
            }

            return result;
        }
    }
}
