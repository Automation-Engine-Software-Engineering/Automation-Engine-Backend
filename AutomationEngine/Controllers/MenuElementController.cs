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
    public class MenuElementController : Controller
    {
        private readonly IMenuElementService _MenuService;
        private readonly TokenGenerator _tokenGenerator;

        public MenuElementController(TokenGenerator tokenGenerator, IMenuElementService MenuService)
        {
            _MenuService = MenuService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateNewRelation([FromBody] MenuElementInsertDTO MenuElement)
        {
            if (MenuElement == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            //is validation model
            if (MenuElement.Id != 0)
                throw new CustomException<MenuElementInsertDTO>(new ValidationDto<MenuElementInsertDTO>(false, "RoleUser", "CorruptedRoleUser", MenuElement), 500);

            var fetchModal = new MenuElement
            {
                Id = 0,
                MenuType = MenuElement.MenuType,
                Name = MenuElement.Name,
                link = MenuElement.link,
                ParentMenuElemntId = MenuElement.ParentMenuElemntId,
                RoleId = MenuElement.RoleId,
                WorkflowId = MenuElement.WorkflowId == 0 ? null : MenuElement.WorkflowId
            };

            await _MenuService.InsertMenuElement(fetchModal);
            var validationModel = await _MenuService.SaveChangesAsync();
            if (!validationModel.IsSuccess)
                throw new CustomException<string>(validationModel, 500);
            return (new ResultViewModel { Data = MenuElement, Message = new ValidationDto<MenuElementInsertDTO>(true, "Success", "Success", MenuElement).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateRoleUser([FromBody] MenuElementInsertDTO MenuElement)
        {
            if (MenuElement == null)
                throw new CustomException<MenuElement>(new ValidationDto<MenuElement>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            var workflow = await _MenuService.GetMenuElementById(MenuElement.Id);

            var result = new MenuElement()
            {
                Id = workflow.Id,
                MenuType = MenuElement.MenuType,
                Name = MenuElement.Name,
                link = MenuElement.link,
                ParentMenuElemntId = MenuElement.ParentMenuElemntId,
                RoleId = MenuElement.RoleId,
                WorkflowId = MenuElement.WorkflowId
            };

            //is validation model
            if (MenuElement.Id == 0)
                throw new CustomException<MenuElement>(new ValidationDto<MenuElement>(false, "RoleUser", "CorruptedRoleUser", result), 500);

            await _MenuService.UpdateMenuElement(result);
            var validationModel = await _MenuService.SaveChangesAsync();
            if (!validationModel.IsSuccess)
                throw new CustomException<string>(validationModel, 500);
            return (new ResultViewModel { Data = result, Message = new ValidationDto<MenuElement>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveRoleUser([FromBody] int MenuElementId)
        {
            //is validation model
            if (MenuElementId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", MenuElementId), 500);

            var fetchForm = await _MenuService.GetMenuElementById(MenuElementId);
            if (fetchForm == null)
                throw new CustomException<MenuElement>(new ValidationDto<MenuElement>(false, "WorkflowRole", "CorruptedWorkflowRole", fetchForm), 500);

            //initial action
            await _MenuService.DeleteMenuElement(MenuElementId);
            var saveResult = await _MenuService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<MenuElement>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all/Role")]
        public async Task<ResultViewModel> GetAllRoleUser()
        {
            var claims = await HttpContext.Authorize();
            var forms = await _MenuService.GetMenuElementByRoleId(claims.RoleId);

            return (new ResultViewModel { Data = forms, Message = new ValidationDto<List<ViewModels.ViewModels.RoleDtos.MenuElementDTO>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllMenuElement(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _MenuService.GetAllMenuElement(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<MenuElement>>(new ValidationDto<ListDto<MenuElement>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<MenuElement>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}
