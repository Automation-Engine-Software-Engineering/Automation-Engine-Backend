using Microsoft.AspNetCore.Mvc;
using Services;
using DataLayer.Models.FormBuilder;
using ViewModels;
using ViewModels.ViewModels.FormBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using DataLayer.Models.TableBuilder;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormController(IFormService formService)
        {
            _formService = formService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateForm([FromBody] FormDto form)
        {
            if (form == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", null), 500);
            //transfer model
            var result = new Form(form.Name, form.Description, form.SizeHeight, form.BackgroundImgPath, form.SizeWidth
            , form.HtmlFormBody, form.IsAutoHeight, form.BackgroundColor, form.IsRepeatedImage);

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
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateForm([FromBody] FormDto form)
        {
            if (form == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", null), 500);

            //transfer model
            var result = new Form(form.Name, form.Description, form.SizeHeight, form.BackgroundImgPath, form.SizeWidth
            , form.HtmlFormBody, form.IsAutoHeight, form.BackgroundColor, form.IsRepeatedImage);

            //is validation model
            if (form.Id == 0)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedForm", result), 500);

            result.Id = form.Id;
            var validationModel = await _formService.FormValidationAsync(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            var fetchForm = await _formService.GetFormByIdAsync(form.Id);
            if (fetchForm == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "CorruptedNotfound", result), 500);

            //initial action
            await _formService.UpdateFormAsync(result);
            var saveResult = await _formService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Form>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
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
            var forms = await _formService.GetAllFormsAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Form>>(new ValidationDto<ListDto<Form>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms, Message = new ValidationDto<ListDto<Form>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{formId}")]
        public async Task<ResultViewModel> GetForm(int formId)
        {
            //is validation model
            if (formId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedForm", formId), 500);

            //initial action
            var form = await _formService.GetFormByIdAsync(formId);
            if (form == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Form", "CorruptedNotfound", formId), 500);

            var validationModel = await _formService.FormValidationAsync(form);
            if (!validationModel.IsSuccess)
                throw new CustomException<Form>(validationModel, 500);

            return (new ResultViewModel { Data = form, Message = new ValidationDto<Form>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
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
    }
}