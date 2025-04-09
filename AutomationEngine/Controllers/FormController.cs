using Microsoft.AspNetCore.Mvc;
using Services;
using DataLayer.Models.FormBuilder;
using ViewModels;
using ViewModels.ViewModels.FormBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using DataLayer.Models.TableBuilder;
using Tools;
using AutomationEngine.ControllerAttributes;
using DataLayer.Models.Enums;
using DataLayer.Models.WorkFlows;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DataLayer.DbContext;
using Tools.AuthoraizationTools;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IWorkFlowUserService _workFlowUserService;
        private readonly IWorkFlowService _workFlowService;
        private readonly IPropertyService _propertyService;
        private readonly DynamicDbContext _dynamicDbContext;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IWorkFlowRoleService _workFlowRoleService;

        public FormController(IFormService formService, IWorkFlowUserService workFlowUserService, IWorkFlowService workFlowService, IPropertyService propertyService, DynamicDbContext dynamicDbContext, TokenGenerator tokenGenerator, IWorkFlowRoleService workFlowRoleService)
        {
            _formService = formService;
            _workFlowUserService = workFlowUserService;
            _workFlowService = workFlowService;
            _propertyService = propertyService;
            _dynamicDbContext = dynamicDbContext;
            _tokenGenerator = tokenGenerator;
            _workFlowRoleService = workFlowRoleService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateForm([FromForm] FormDto form)
        {
            if (form == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", null), 500);

            //transfer model
            var result = new Form(form.Name, form.Description, form.HtmlFormBody);

            //is validation model
            if (form.Id != 0)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", result), 500);

            var validationModel = await _formService.FormValidationAsync(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            //initial action
            result.Entities = new List<Entity>();
            await _formService.CreateFormAsync(result);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Form>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("{formId}/update")]
        public async Task<ResultViewModel> UpdateForm(int formId, [FromForm] UpdateFormInputModel form)
        {
            if (form == null)
                throw new CustomException<UpdateFormInputModel>(new ValidationDto<UpdateFormInputModel>(false, "Form", "CorruptedForm", form), 500);

            var fetchForm = await _formService.GetFormByIdIncEntityAsync(formId);
            if (fetchForm == null)
                throw new CustomException<UpdateFormInputModel>(new ValidationDto<UpdateFormInputModel>(false, "Form", "CorruptedNotfound", form), 500);

            //is validation model
            if (formId == 0)
                throw new CustomException<UpdateFormInputModel>(new ValidationDto<UpdateFormInputModel>(false, "Form", "CorruptedForm", form), 500);

            //if (form.BackgroundImg != null)
            //    RemoveFile.Remove(fetchForm.BackgroundImgPath);
            //transfer model

            var result = new Form(
                form.Name ?? fetchForm.Name,
                form.Description ?? fetchForm.Description,
                form.HtmlFormBody ?? fetchForm.HtmlFormBody
             );
            result.Id = formId;

            var validationModel = await _formService.FormValidationAsync(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);



            //initial action
            await _formService.UpdateFormAsync(result);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Form>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/{formId}/updateEntities
        [HttpPost("entities")]
        public async Task<ResultViewModel> UpdateFormEntities(IEnumerable<int>? Entities, int formId)
        {
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            var fetchForm = await _formService.IsFormExistAsync(formId);
            if (fetchForm == false)
                throw new CustomException<IEnumerable<int>>(new ValidationDto<IEnumerable<int>>(false, "Form", "CorruptedNotfound", Entities), 500);

            await _formService.AddEntitiesToFormAsync(formId, Entities?.ToList() ?? new List<int>());

            var saveResult = await _formService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Message = new ValidationDto<IEnumerable<int>>(true, "Success", "Success", Entities).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm([FromBody] int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            var fetchForm = await _formService.GetFormByIdAsync(formId);
            if (fetchForm == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var validationModel = await _formService.FormValidationAsync(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            //initial action
            await _formService.RemoveFormAsync(formId);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<Form>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllForms(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _formService.GetAllFormsAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Form>>(new ValidationDto<ListDto<Form>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Form>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{formId}")]
        public async Task<ResultViewModel> GetForm(int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            //initial action
            var form = await _formService.GetFormByIdIncEntityIncPropertyAsync(formId);
            if (form == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var validationModel = await _formService.FormValidationAsync(form);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            return (new ResultViewModel { Data = form, Message = new ValidationDto<Form>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/{id}  
        [HttpGet("preview")]
        public async Task<ResultViewModel> GetFormPreview(int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            //initial action
            var form = await _formService.GetFormByIdAsync(formId);
            var formBody = await _formService.GetFormpreview(form);
            if (formBody == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            return (new ResultViewModel { Data = formBody, Message = new ValidationDto<string>(true, "Success", "Success", formBody).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/{id}  
        [HttpGet("previewByWorkFlowUserId")]
        public async Task<ResultViewModel> GetPreviewByWorkFlowUserId(int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", workflowUserId), 500);

            //initial action
            var workflowUser = await _workFlowUserService.GetWorFlowUserById(workflowUserId);
            var node = workflowUser.WorkFlow.Nodes.FirstOrDefault(n => n.Id == workflowUser.WorkFlowState);
            var form = await _formService.GetFormByIdAsync(node.FormId.Value);
            var formBody = await _formService.GetFormpreview(form);
            if (formBody == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", node.FormId.Value), 500);

            return (new ResultViewModel { Data = formBody, Message = new ValidationDto<string>(true, "Success", "Success", formBody).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/uploadImage  
        [HttpPost("uploadImage")]
        public async Task<ResultViewModel> UploadImageFormContent(IFormFile image)
        {
            //is validation model
            //if (formId == 0)
            //    throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            ////initial action
            //var form = await _formService.IsFormExistAsync(formId);
            //if (!form)
            //    throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var imageUrl = await UploadImage.UploadFormMedia(image);

            return (new ResultViewModel { Data = new { imageUrl }, Message = new ValidationDto<string>(true, "Success", "Success", imageUrl).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/insertHtmlContent")]
        public async Task<ResultViewModel> InsertHtmlContent(int formId, [FromBody] string htmlContent)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            var form = await _formService.GetFormByIdAsync(formId);
            if (form == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var validationModel = await _formService.FormValidationAsync(form);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            await _formService.UpdateFormBodyAsync(formId, htmlContent);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = form, Message = new ValidationDto<Form>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/sendFormData")]
        public async Task<ResultViewModel> SendFormData(int formId, int workflowUserId, [FromBody] List<(int id, object content)> formData)
        {
            if (formId == 0 && workflowUserId == 0 && formData.Any(x => x.id == 0))
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedFormData", formId), 500);

            var workflowUser = await _workFlowUserService.GetWorFlowUserById(workflowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "NoUserWorkflowFound", workflowUserId), 500);

            var form = await _formService.GetFormByIdIncEntityIncPropertyAsync(formId);
            if (form == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var claims = await HttpContext.Authorize();

            if (claims.UserId != workflowUser.UserId)
                throw new CustomException<int>(new ValidationDto<int>(false, "User", "CorruptedUser", formId), 500);

            var workflow = await _workFlowService.GetWorFlowByIdIncNodesAsync(workflowUser.WorkFlowId);
            if (workflow == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowRoleFound", formId), 500);

            var workflowRole = await _workFlowRoleService.ExistAllWorFlowRolesBuRoleId(claims.RoleId, workflow.Id);
            if (!workflowRole)
                throw new CustomException<int>(new ValidationDto<int>(false, "Warning", "NotAuthorized", formId), 500);

            var currentNode = workflow.Nodes.FirstOrDefault(x => x.Id == workflowUser.WorkFlowState);
            if (currentNode.FormId != formId)
                throw new CustomException<Node>(new ValidationDto<Node>(false, "Form", "CorruptedNotfound", currentNode), 500);

            List<Entity> entites = new List<Entity>();
            foreach (var prop in formData)
            {
                var property = await _propertyService.GetColumnByIdAsync(prop.id);


                if (entites.Any(x => x == property.Entity))
                {
                    entites.FirstOrDefault(x => x == property.Entity)
                    .Properties.Add(property);
                }
                else
                {
                    var entity = property.Entity;
                    entity.Properties = new List<EntityProperty>();
                    entity.Properties.Add(property);
                }
            }

            foreach (var entity in entites)
            {
                string query = $"Insert into {entity.TableName} (";
                int i = 0;
                var propName = new List<string>();
                var propValue = new List<string>();

                entity.Properties.ForEach(x =>
                {
                    propName.Add(x.PropertyName);
                    propValue.Add(formData.FirstOrDefault(xx => xx.id == x.Id).content.ToString());
                });

                i = 0;
                propName.ForEach(x =>
                {
                    if (i != 0)
                        query += " , ";
                    query += x;
                });

                query += ") Value (";

                i = 0;
                propValue.ForEach(x =>
                {
                    if (i != 0)
                        query += " , ";
                    query += x;
                });

                query += ")";

                _dynamicDbContext.ExecuteSqlRawAsync(query);
            }

            return (new ResultViewModel { Data = entites, Message = new ValidationDto<Form>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}