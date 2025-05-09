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
            var query1 = "select COUNT(*)  as c from [dbo].[DraftContract]";
            var query2 = "select COUNT(*)  as c from [dbo].[ExternalContract]";
            var query3 = "select COUNT(*)  as c from [dbo].[InternalContract]";
            var query4 = "select COUNT(*)  as c from [dbo].[AllArticles] where IsRate is null or IsRate = N'نامشخص' ";
            var query5 = "select COUNT(*)  as c from [dbo].[AllArticles] where IsRate is null or IsRate = N'تایید'";
            var query6 = "select COUNT(*)  as c from [dbo].[AllArticles] where IsRate is null or IsRate = N'عدم تایید'";


            var data1 = await _dynamicDbContext.ExecuteReaderAsync(query1);
            var data2 = await _dynamicDbContext.ExecuteReaderAsync(query2);
            var data3 = await _dynamicDbContext.ExecuteReaderAsync(query3);
            var data4 = await _dynamicDbContext.ExecuteReaderAsync(query4);
            var data5 = await _dynamicDbContext.ExecuteReaderAsync(query5);
            var data6 = await _dynamicDbContext.ExecuteReaderAsync(query6);
            if (int.Parse(data1.Data.ToList()[0]["c"].ToString()) > 0)
            {
                result.Add($"{int.Parse(data1.Data.ToList()[0]["c"].ToString())} عنصر جدید به پیش نویس‌ها اضافه شده است");
            }

              if (int.Parse(data2.Data.ToList()[0]["c"].ToString()) > 0)
            {    result.Add($"{int.Parse(data2.Data.ToList()[0]["c"].ToString())} عنصر جدید به قرارداد برون دانشگاهی اضافه شده است");
            }

              if (int.Parse(data3.Data.ToList()[0]["c"].ToString()) > 0)
            {    result.Add($"{int.Parse(data3.Data.ToList()[0]["c"].ToString())} عنصر جدید به درون دانشگاهی اضافه شده است");
            }
             if (int.Parse(data4.Data.ToList()[0]["c"].ToString()) > 0)
            {     result.Add($"{int.Parse(data4.Data.ToList()[0]["c"].ToString())} عنصر جدید به مقالات در انتظار تایید مالکیت اضافه شده");
            }
              if (int.Parse(data5.Data.ToList()[0]["c"].ToString()) > 0)
            {    result.Add($"{int.Parse(data5.Data.ToList()[0]["c"].ToString())} عنصر جدید به مقالات تایید شده اضافه شده است");
            }
              if (int.Parse(data6.Data.ToList()[0]["c"].ToString()) > 0)
            {    result.Add($"{int.Parse(data6.Data.ToList()[0]["c"].ToString())} عنصر جدید به مقالات تایید نشده اضافه شده است");
            }

            return result;
        }
    }
}
