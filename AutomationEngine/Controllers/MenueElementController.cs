using Entities.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using Entities.Models.Workflows;
using Services;
using ViewModels.ViewModels.Workflow;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
using Entities.Models.MainEngine;
using Tools.AuthoraizationTools;
using Entities.Models.Enums;
using ViewModels.ViewModels.RoleDtos;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class MenueElementController : Controller
    {
        private readonly IMenueElementService _menueService;
        private readonly TokenGenerator _tokenGenerator;

        public MenueElementController(TokenGenerator tokenGenerator, IMenueElementService menueService)
        {
            _menueService = menueService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateNewRelation([FromBody] MenueElementInsertDTO MenueElement)
        {
            if (MenueElement == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            //is validation model
            if (MenueElement.Id != 0)
                throw new CustomException<MenueElementInsertDTO>(new ValidationDto<MenueElementInsertDTO>(false, "RoleUser", "CorruptedRoleUser", MenueElement), 500);

            var fetchModal = new MenueElement
            {
                Id = 0,
                MenueType = MenueElement.MenueType,
                Name = MenueElement.Name,
                ParentMenueElemntId = MenueElement.ParentMenueElemntId,
                RoleId = MenueElement.RoleId,
                WorkflowId = MenueElement.WorkflowId == 0 ? null : MenueElement.WorkflowId
            };

            await _menueService.InsertMenueElement(fetchModal);
            var validationModel = await _menueService.SaveChangesAsync();
            if (!validationModel.IsSuccess)
                throw new CustomException<string>(validationModel, 500);
            return (new ResultViewModel { Data = MenueElement, Message = new ValidationDto<MenueElementInsertDTO>(true, "Success", "Success", MenueElement).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateRoleUser([FromBody] MenueElementInsertDTO MenueElement)
        {
            if (MenueElement == null)
                throw new CustomException<MenueElement>(new ValidationDto<MenueElement>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            var workflow = await _menueService.GetMenueElementById(MenueElement.Id);

            var result = new MenueElement()
            {
                Id = workflow.Id,
                MenueType = MenueElement.MenueType,
                Name = MenueElement.Name,
                ParentMenueElemntId = MenueElement.ParentMenueElemntId,
                RoleId = MenueElement.RoleId,
                WorkflowId = MenueElement.WorkflowId
            };

            //is validation model
            if (MenueElement.Id == 0)
                throw new CustomException<MenueElement>(new ValidationDto<MenueElement>(false, "RoleUser", "CorruptedRoleUser", result), 500);

            await _menueService.UpdateMenueElement(result);
            var validationModel = await _menueService.SaveChangesAsync();
            if (!validationModel.IsSuccess)
                throw new CustomException<string>(validationModel, 500);
            return (new ResultViewModel { Data = result, Message = new ValidationDto<MenueElement>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveRoleUser([FromBody] int MenueElementId)
        {
            //is validation model
            if (MenueElementId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", MenueElementId), 500);

            var fetchForm = await _menueService.GetMenueElementById(MenueElementId);
            if (fetchForm == null)
                throw new CustomException<MenueElement>(new ValidationDto<MenueElement>(false, "WorkflowRole", "CorruptedWorkflowRole", fetchForm), 500);

            //initial action
            await _menueService.DeleteMenueElement(MenueElementId);
            var saveResult = await _menueService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<MenueElement>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllRoleUser()
        {
            var claims = await HttpContext.Authorize();
            var forms = await _menueService.GetMenueElementByRoleId(claims.RoleId);

            return (new ResultViewModel { Data = forms, Message = new ValidationDto<List<ViewModels.ViewModels.RoleDtos.MenueElementDTO>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
