using Microsoft.AspNetCore.Mvc;
using Services;
using Entities.Models.FormBuilder;
using ViewModels;
using ViewModels.ViewModels.FormBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Entities.Models.TableBuilder;
using Tools;
using AutomationEngine.ControllerAttributes;
using Entities.Models.Enums;
using Entities.Models.Workflows;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DataLayer.DbContext;
using Tools.AuthoraizationTools;
using Tools.TextTools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IWorkflowUserService _workflowUserService;
        private readonly IWorkflowService _workflowService;
        private readonly IPropertyService _propertyService;
        private readonly DynamicDbContext _dynamicDbContext;
        private readonly IWorkflowRoleService _workflowRoleService;

        public FormController(IFormService formService, IWorkflowUserService workflowUserService, IWorkflowService workflowService, IPropertyService propertyService, DynamicDbContext dynamicDbContext, IWorkflowRoleService workflowRoleService)
        {
            _formService = formService;
            _workflowUserService = workflowUserService;
            _workflowService = workflowService;
            _propertyService = propertyService;
            _dynamicDbContext = dynamicDbContext;
            _workflowRoleService = workflowRoleService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel<Form?>> CreateForm([FromForm] FormDto form)
        {
            if (form == null)
                throw new CustomException("Form", "CorruptedForm");

            //transfer model
            var result = new Form(form.Name, form.Description, form.HtmlFormBody);

            //is validation model
            if (form.Id != 0)
                throw new CustomException("Form", "CorruptedForm");

            var validationModel = _formService.FormValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            result.Entities = new List<Entity>();
            await _formService.CreateFormAsync(result);
            await _formService.SaveChangesAsync();

            return new ResultViewModel<Form?>(result);
        }

        // POST: api/form/update  
        [HttpPost("{formId}/update")]
        public async Task<ResultViewModel<Form?>> UpdateForm(int formId, [FromForm] UpdateFormInputModel form)
        {
            if (form == null)
                throw new CustomException("Form", "CorruptedForm", form);

            var fetchForm = await _formService.GetFormByIdIncEntityAsync(formId);
            if (fetchForm == null)
                throw new CustomException("Form", "FormNotfound", form);

            //is validation model
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm", form);

            //if (form.BackgroundImg != null)
            //    RemoveFile.Remove(fetchForm.BackgroundImgPath);
            //transfer model

            var result = new Form(
                form.Name ?? fetchForm.Name,
                form.Description ?? fetchForm.Description,
                form.HtmlFormBody ?? fetchForm.HtmlFormBody
             );
            result.Id = formId;

            var validationModel = _formService.FormValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;



            //initial action
            await _formService.UpdateFormAsync(result);
            await _formService.SaveChangesAsync();

            return new ResultViewModel<Form?> (result);
        }

        // POST: api/form/{formId}/updateEntities
        [HttpPost("entities")]
        public async Task<ResultViewModel<object>> UpdateFormEntities(IEnumerable<int>? Entities, int formId)
        {
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm");

            var fetchForm = await _formService.IsFormExistAsync(formId);
            if (fetchForm == false)
                throw new CustomException("Form", "FormNotfound");

            await _formService.AddEntitiesToFormAsync(formId, Entities?.ToList() ?? new List<int>());

            await _formService.SaveChangesAsync();

            return new ResultViewModel<object>() ;
        }

        // POST: api/form/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Form?>> RemoveForm([FromBody] int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm", formId);

            var fetchForm = await _formService.GetFormByIdAsync(formId);
            if (fetchForm == null)
                throw new CustomException("Form", "FormNotfound", formId);

            var validationModel = _formService.FormValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            await _formService.RemoveFormAsync(formId);
            await _formService.SaveChangesAsync();

            return new ResultViewModel<Form?>(fetchForm);
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<Form>?>> GetAllForms(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _formService.GetAllFormsAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<Form>?> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount };
        }

        // GET: api/form/{id}  
        [HttpGet("{formId}")]
        public async Task<ResultViewModel<Form?>> GetForm(int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm", formId);

            //initial action
            var form = await _formService.GetFormByIdIncEntityIncPropertyAsync(formId);
            if (form == null)
                throw new CustomException("Form", "FormNotfound", formId);

            var validationModel = _formService.FormValidation(form);
            if (!validationModel.IsSuccess)
                throw validationModel;

            return new ResultViewModel<Form?>(form);
        }


        // GET: api/form/{id}  
        [HttpGet("preview")]
        public async Task<ResultViewModel<string?>> GetFormPreview(int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm", formId);

            //initial action
            var form = await _formService.GetFormByIdAsync(formId);
            var formBody = await _formService.GetFormPreviewAsync(form, 0);
            if (formBody == null)
                throw new CustomException("Form", "FormNotfound", formId);

            return new ResultViewModel<string?>(formBody);
        }


        // GET: api/form/{id}  
        [HttpGet("previewByWorkflowUserId")]
        public async Task<ResultViewModel<string?>> GetPreviewByWorkflowUserId(int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException("Form", "CorruptedForm", workflowUserId);

            //initial action
            var workflowUser = await _workflowUserService.GetWorkflowUserById(workflowUserId);
            var node = workflowUser.Workflow.Nodes.FirstOrDefault(n => n.Id == workflowUser.WorkflowState);
            var form = await _formService.GetFormByIdIncEntityIncPropertyAsync(node.FormId.Value);
            var formBody = await _formService.GetFormPreviewAsync(form, workflowUserId);
            if (formBody == null)
                throw new CustomException("Form", "FormNotfound", node.FormId.Value);

            return new ResultViewModel<string?>(formBody);
        }

        // GET: api/form/uploadImage  
        [HttpPost("uploadImage")]
        public async Task<ResultViewModel<ImageModel>> UploadImageFormContent(IFormFile image)
        {
            var imageUrl = await UploadImage.UploadFormMedia(image);

            return new ResultViewModel<ImageModel>(new ImageModel { ImageUrl = imageUrl });
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/insertHtmlContent")]
        public async Task<ResultViewModel<Form?>> InsertHtmlContent(int formId, [FromBody] string htmlContent)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException("Form", "CorruptedForm", formId);

            var form = await _formService.GetFormByIdAsync(formId);
            if (form == null)
                throw new CustomException("Form", "FormNotfound", formId);

            var validationModel = _formService.FormValidation(form);
            if (!validationModel.IsSuccess)
                throw validationModel;

            await _formService.UpdateFormBodyAsync(formId, htmlContent);
            await _formService.SaveChangesAsync();

            return new ResultViewModel<Form?>(form);
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("saveFormData")]
        public async Task<ResultViewModel<List<SaveDataDTO>>> SaveFormData(int workflowUserId, [FromBody] List<SaveDataDTO> formData)
        {
            if (workflowUserId == 0 && formData.Any(x => x.id == 0))
                throw new CustomException("Form", "CorruptedFormData");

            var workflowUser = await _workflowUserService.GetWorkflowUserById(workflowUserId);
            if (workflowUser == null)
                throw new CustomException("UserWorkflow", "UserWorkflowNotfound", workflowUserId);

            var claims = await HttpContext.Authorize();
            if (claims.UserId != workflowUser.UserId)
                throw new CustomException("User", "UserNotFound");

            var workflow = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowUser.WorkflowId);
            if (workflow == null)
                throw new CustomException("Workflow", "WorkflowRoleNotfound");

            var workflowRole = await _workflowRoleService.ExistAllWorkflowRolesBuRoleId(claims.RoleId, workflow.Id);
            if (!workflowRole)
                throw new CustomException("Warning", "NotAuthorized");

            await _formService.SaveFormData(workflowUserId, formData);
            return new ResultViewModel<List<SaveDataDTO>>(formData);
        }
    }
}