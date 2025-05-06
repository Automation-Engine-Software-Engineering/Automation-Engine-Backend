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
        public async Task<ResultViewModel<MenuElementInsertDTO>> CreateNewRelation([FromBody] MenuElementInsertDTO MenuElement)
        {
            if (MenuElement == null)
                throw new CustomException("RoleUser", "CorruptedRoleUser");

            //is validation model
            if (MenuElement.Id != 0)
                throw new CustomException("RoleUser", "CorruptedRoleUser", MenuElement);

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
            await _MenuService.SaveChangesAsync();
            
            return new ResultViewModel<MenuElementInsertDTO>(MenuElement);
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<MenuElement?>> UpdateRoleUser([FromBody] MenuElementInsertDTO MenuElement)
        {
            if (MenuElement == null)
                throw new CustomException("RoleUser", "CorruptedRoleUser");

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
                throw new CustomException("RoleUser", "CorruptedRoleUser");

            await _MenuService.UpdateMenuElement(result);
            await _MenuService.SaveChangesAsync();
            return new ResultViewModel<MenuElement?> (result);
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<MenuElement?>> RemoveRoleUser([FromBody] int MenuElementId)
        {
            //is validation model
            if (MenuElementId == 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            var fetchForm = await _MenuService.GetMenuElementById(MenuElementId);
            if (fetchForm == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            //initial action
            await _MenuService.DeleteMenuElement(MenuElementId);
            await _MenuService.SaveChangesAsync();

            return new ResultViewModel<MenuElement?>(fetchForm);
        }

        // GET: api/form/all  
        [HttpGet("all/Role")]
        public async Task<ResultViewModel<IEnumerable<MenuElementDTO>>> GetAllRoleUser()
        {
            var claims = await HttpContext.Authorize();
            var forms = await _MenuService.GetMenuElementByRoleId(claims.RoleId);

            return new ResultViewModel<IEnumerable<MenuElementDTO>>(forms);
        }


        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<MenuElement>>> GetAllMenuElement(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _MenuService.GetAllMenuElement(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage");

            return new ResultViewModel<IEnumerable<MenuElement>>(forms.Data,forms.ListNumber,forms.ListSize,forms.TotalCount);
        }

    }
}
