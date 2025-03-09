using Microsoft.AspNetCore.Mvc;
using Services;
using DataLayer.Models.FormBuilder;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using ViewModels;
using Microsoft.IdentityModel.Tokens;
using ViewModels.ViewModels.FormBuilder;

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
            var result = new Form()
            {
                Name = form.Name,
                BackgroundColor = form.BackgroundColor,
                Description = form.Description,
                BackgroundImgPath = form.BackgroundImgPath,
                HtmlFormBody = form.HtmlFormBody,
                SizeHeight = form.SizeHeight,
                SizeWidth = form.SizeWidth
            };

            await _formService.CreateFormAsync(result);
            await _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateForm([FromBody] FormDto form)
        {
            var result = new Form()
            {
                Id = form.Id,
                Name = form.Name,
                BackgroundColor = form.BackgroundColor,
                Description = form.Description,
                BackgroundImgPath = form.BackgroundImgPath,
                HtmlFormBody = form.HtmlFormBody,
                SizeHeight = form.SizeHeight,
                SizeWidth = form.SizeHeight
            };

            await _formService.UpdateFormAsync(result);
            await _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm([FromBody] int formId)
        {
            await _formService.RemoveFormAsync(formId);
            _formService.SaveChangesAsync();
            return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد", Status = true });
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
            var form = await _formService.GetFormAsync(formId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/insertHtmlContent")]
        public async Task<ResultViewModel> InsertHtmlContent( int formId, [FromBody]string htmlContent)
        {
            await _formService.UpdateFormBodyAsync(formId, htmlContent);
            _formService.SaveChangesAsync();
            return (new ResultViewModel { Message = "عملیات با موفقیت انجام شد", Status = true });
        }
    }
}