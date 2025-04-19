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
        public async Task<ResultViewModel> CreateForm([FromForm] FormDto form)
        {
            if (form == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", null), 500);

            //transfer model
            var result = new Form(form.Name, form.Description, form.HtmlFormBody);

            //is validation model
            if (form.Id != 0)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", result), 500);

            var validationModel = _formService.FormValidation(result);
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
                throw new CustomException<UpdateFormInputModel>(new ValidationDto<UpdateFormInputModel>(false, "Form", "FormNotfound", form), 500);

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

            var validationModel = _formService.FormValidation(result);
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
                throw new CustomException<IEnumerable<int>>(new ValidationDto<IEnumerable<int>>(false, "Form", "FormNotfound", Entities), 500);

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
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "FormNotfound", formId), 500);

            var validationModel = _formService.FormValidation(fetchForm);
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
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "FormNotfound", formId), 500);

            var validationModel = _formService.FormValidation(form);
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
            var formBody = await _formService.GetFormPreviewAsync(form);
            if (formBody == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "FormNotfound", formId), 500);

            return (new ResultViewModel { Data = formBody, Message = new ValidationDto<string>(true, "Success", "Success", formBody).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/{id}  
        [HttpGet("previewByWorkflowUserId")]
        public async Task<ResultViewModel> GetPreviewByWorkflowUserId(int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", workflowUserId), 500);

            //initial action
            var workflowUser = await _workflowUserService.GetWorkflowUserById(workflowUserId);
            var node = workflowUser.Workflow.Nodes.FirstOrDefault(n => n.Id == workflowUser.WorkflowState);
            var form = await _formService.GetFormByIdAsync(node.FormId.Value);
            var formBody = await _formService.GetFormPreviewAsync(form);
            if (formBody == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "FormNotfound", node.FormId.Value), 500);

            return (new ResultViewModel { Data = formBody, Message = new ValidationDto<string>(true, "Success", "Success", formBody).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/uploadImage  
        [HttpPost("uploadImage")]
        public async Task<ResultViewModel> UploadImageFormContent(IFormFile image)
        {
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
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "FormNotfound", formId), 500);

            var validationModel = _formService.FormValidation(form);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            await _formService.UpdateFormBodyAsync(formId, htmlContent);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = form, Message = new ValidationDto<Form>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // // POST: api/form/{formId}/updateBody  
        // [HttpPost("saveFormData")]
        // public async Task<ResultViewModel> SaveFormData(int workflowUserId, [FromBody] List<SaveDataDTO> formData)
        // {
        //     if (workflowUserId == 0 && formData.Any(x => x.id == 0))
        //         throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedFormData", workflowUserId), 500);

        //     var workflowUser = await _workflowUserService.GetWorkflowUserById(workflowUserId);
        //     if (workflowUser == null)
        //         throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "UserWorkflowNotfound", workflowUserId), 500);

        //     var claims = await HttpContext.Authorize();

        //     if (claims.UserId != workflowUser.UserId)
        //         throw new CustomException<int>(new ValidationDto<int>(false, "User", "UserNotFound", workflowUserId), 500);

        //     var workflow = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowUser.WorkflowId);
        //     if (workflow == null)
        //         throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowRoleNotfound", workflowUserId), 500);

        //     var workflowRole = await _workflowRoleService.ExistAllWorkflowRolesBuRoleId(claims.RoleId, workflow.Id);
        //     if (!workflowRole)
        //         throw new CustomException<int>(new ValidationDto<int>(false, "Warning", "NotAuthorized", workflowUserId), 403);


        //     List<Entity> entites = new List<Entity>();
        //     foreach (var prop in formData)
        //     {
        //         var property = new EntityProperty();
        //         property = await _propertyService.GetColumnByIdIncEntityAsync(prop.id);
        //         if (property == null)
        //             throw new CustomException<int>(new ValidationDto<int>(false, "Property", "PropertyNotFound", workflowUserId), 500);

        //         if (entites.Any(x => property != null && x == property.Entity && x.PreviewName == prop.group))
        //         {
        //             entites.First(x => x == property?.Entity && x.PreviewName == prop.group).Properties?.Add(property);
        //         }
        //         else
        //         {
        //             var entity = property.Entity;
        //             entity.PreviewName = prop.group;
        //             if (entity == null)
        //                 throw new CustomException<int>(new ValidationDto<int>(false, "Entity", "EntityNotFound", workflowUserId), 500);

        //             entity.Properties = [property];
        //             entites.Add(entity);
        //         }
        //     }

        //     foreach (var entity in entites)
        //     {
        //         string query = $"Insert into [dbo].[{entity.TableName}] (";
        //         int i = 0;
        //         var propName = new List<string>();
        //         var propValue = new List<string>();

        //         entity.Properties?.ForEach(x =>
        //         {
        //             propName.Add(x.PropertyName);
        //             propValue.Add(formData.FirstOrDefault(xx => xx.id == x.Id).content.ToString() ?? "");
        //         });

        //         propValue.ForEach(x => x.IsValidString());
        //         i = 0;
        //         propName.ForEach(x =>
        //         {
        //             if (i != 0)
        //                 query += " , ";
        //             query += x;
        //             i++;
        //         });

        //         query += ") Values (";

        //         i = 0;
        //         propValue.ForEach(x =>
        //         {
        //             if (i != 0)
        //                 query += " , ";
        //             query += $"N'{x}'";
        //             i++;

        //         });

        //         query += ")";

        //         await _dynamicDbContext.ExecuteSqlRawAsync(query);
        //     }

        //     return (new ResultViewModel { Data = entites, Message = new ValidationDto<List<Entity>>(true, "Success", "Success", entites).GetMessage(200), Status = true, StatusCode = 200 });
        // }


        // POST: api/form/{formId}/updateBody  
        [HttpPost("saveFormData")]
        public async Task<ResultViewModel> SaveFormData(int workflowUserId, [FromBody] List<SaveDataDTO> formData)
        {
            if (workflowUserId == 0 && formData.Any(x => x.id == 0))
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedFormData", workflowUserId), 500);

            var workflowUser = await _workflowUserService.GetWorkflowUserById(workflowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "UserWorkflowNotfound", workflowUserId), 500);

            var claims = await HttpContext.Authorize();

            if (claims.UserId != workflowUser.UserId)
                throw new CustomException<int>(new ValidationDto<int>(false, "User", "UserNotFound", workflowUserId), 500);

            var workflow = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowUser.WorkflowId);
            if (workflow == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowRoleNotfound", workflowUserId), 500);

            var workflowRole = await _workflowRoleService.ExistAllWorkflowRolesBuRoleId(claims.RoleId, workflow.Id);
            if (!workflowRole)
                throw new CustomException<int>(new ValidationDto<int>(false, "Warning", "NotAuthorized", workflowUserId), 403);


            List<Entity> entites = new List<Entity>();
            foreach (var prop in formData)
            {
                var property = await _propertyService.GetColumnByIdIncEntityAsync(prop.id);
                if (property == null)
                    throw new CustomException<int>(new ValidationDto<int>(false, "Property", "PropertyNotFound", workflowUserId), 500);

                if (!entites.Any(x => property != null && x == property.Entity))
                {
                    var entity = property.Entity;
                    if (entity == null)
                        throw new CustomException<int>(new ValidationDto<int>(false, "Entity", "EntityNotFound", workflowUserId), 500);

                    entity.Properties = [property];
                    entites.Add(entity);
                }
            }

            foreach (var entity in entites)
            {
                var countQuery = $"SELECT TOP(1) id FROM [dbo].[{entity.TableName}] ORDER BY id DESC";
                var data = await _dynamicDbContext.ExecuteReaderAsync(countQuery);
                string query = $"Insert into [dbo].[{entity.TableName}] (";
                int i = 0;
                var propName = new List<string>();
                var propValue = new List<string>();

                entity.Properties?.ForEach(x =>
                {
                    propName.Add(x.PropertyName);
                    propValue.Add(formData.FirstOrDefault(xx => xx.id == x.Id).content.ToString() ?? "");
                });

                propValue.ForEach(x => x.IsValidString());
                i = 0;
                query += "Id";
                query += " , WorkflowUserId";


                propName.ForEach(x =>
                {
                    query += " , ";
                    query += x;
                    i++;
                });

                query += ") Values (";

                i = 0;
                if (data.Data.ToList().Count == 0)
                {
                    query += "1";
                }
                else
                {
                    query += int.Parse(data.Data.ToList()[0]["id"].ToString()) + 1;
                }
                query += " , " + workflowUserId;

                propValue.ForEach(x =>
                {
                    query += " , ";
                    query += $"N'{x}'";
                    i++;

                });

                query += ")";

                await _dynamicDbContext.ExecuteSqlRawAsync(query);
            }

            return (new ResultViewModel { Data = entites, Message = new ValidationDto<List<Entity>>(true, "Success", "Success", entites).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}