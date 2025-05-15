using AutomationEngine.ControllerAttributes;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using Tools.TextTools;
using ViewModels;
using ViewModels.ViewModels.Entity;
using ViewModels.ViewModels.Workflow;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;


        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("all")]
        public async Task<ResultViewModel> Notification()
        {
            var rolId = (await HttpContext.Authorize()).RoleId;
            var result = await _notificationService.GetallNotification(rolId);
            return (new ResultViewModel { Data = result, Message = new ValidationDto<List<string>>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}