using Microsoft.AspNetCore.Mvc;
using Services;
using DataLayer.Models.FormBuilder;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using ViewModels;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<ResultViewModel> CreateForm([FromBody] Form form)
        {
            if (form == null)
                throw new ArgumentNullException("فرم یافت نشد");

            await _formService.CreateFormAsync(form);
            await _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateForm([FromBody] Form form)
        {
            if (form == null)
                throw new ArgumentNullException("فرم یافت نشد");

            await _formService.UpdateFormAsync(form);
            await _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm(int formId)
        {
            if (formId == null)
                throw new ArgumentNullException("فرم یافت نشد");

            var form = await _formService.GetFormAsync(formId);
            await _formService.RemoveFormAsync(form);
            _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return (new ResultViewModel { Data = forms, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{formId}")]
        public async Task<ResultViewModel> GetForm(int formId)
        {
            if (formId == null)
                throw new ArgumentNullException("فرم یافت نشد");

            var form = await _formService.GetFormAsync(formId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/insertHtmlContent")]
        public async Task<ResultViewModel> InsertHtmlContent( int formId, [FromBody]string htmlContent)
        {
            if (formId == null)
                throw new ArgumentNullException("فرم یافت نشد");

            if (htmlContent.IsNullOrEmpty())
                throw new ArgumentNullException("فرم یافت نشد");

            await _formService.UpdateFormBodyAsync(formId, htmlContent);
            _formService.SaveChangesAsync();
            return (new ResultViewModel { Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        //// POST: api/formbuilder/submit  
        //[HttpPost("submit")]
        //public async Task<ResultViewModel> SubmitForm([FromBody] FormData formData)
        //{
        //    if (formData == null || formData.Fields == null || formData.Fields.Count == 0)
        //    {
        //        return BadRequest("Invalid form data.");
        //    }

        //    try
        //    {
        //        foreach (var field in formData.Fields)
        //        {
        //            // Assuming the service method is responsible for determining which DB operation to perform  
        //            await _formService.InsertFieldValueAsync(field.TableName, field.FieldName, field.Value);
        //        }

        //        return Ok("Form data submitted successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }
}