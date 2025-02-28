using Microsoft.AspNetCore.Mvc;
using Services;
using DataLayer.Models.FormBuilder; // Update accordingly  
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using ViewModels;

namespace AutomationEngine.Controllers // Replace with your actual namespace  
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
        public async Task<IActionResult> CreateForm([FromBody] Form form)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _formService.CreateFormAsync(form);
            await _formService.SaveChangesAsync(); // Ensure changes are saved  

            return CreatedAtAction(nameof(GetForm), new { id = form.Id }, form);
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<IActionResult> EditForm([FromBody] Form form)
        {
            if (form == null || !ModelState.IsValid)
            {
                return BadRequest("Form data is invalid.");
            }

            try
            {
                await _formService.EditFormAsync(form);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Form with ID {form.Id} not found.");
            }

            return NoContent(); // 204 No Content  
        }

        // POST: api/form/delete  
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteForm([FromBody] int formId)
        {
            try
            {
                var form = await _formService.GetFormAsync(formId);
                await _formService.DeleteFormAsync(form);
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Form with ID {formId} not found.");
            }
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ActionResult<List<Form>>> GetAllForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms); // 200 OK  
        }

        // GET: api/form/{id}  
        [HttpGet("{id}")]
        public async Task<ActionResult<Form>> GetForm(int id)
        {
            try
            {
                var form = await _formService.GetFormAsync(id);
                return Ok(form); // 200 OK  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Form with ID {id} not found."); // 404 Not Found  
            }
        }

        // POST: api/form/{formId}/updateBody  
        [HttpPost("{formId}/insertHtmlContent")]
        public async Task<IActionResult> InsertHtmlContent(int formId, [FromBody] string htmlContent)
        {
            // Validate Input  
            if (formId <= 0)
            {
                return BadRequest("Invalid form ID.");
            }

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return BadRequest("HTML content cannot be empty.");
            }

            try
            {
                // Update the HtmlFormBody field in the database  
                await _formService.UpdateFormBodyAsync(formId, htmlContent);

                return NoContent(); // 204 No Content - Successfully updated  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Form with ID '{formId}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/formbuilder/submit  
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForm([FromBody] FormData formData)
        {
            if (formData == null || formData.Fields == null || formData.Fields.Count == 0)
            {
                return BadRequest("Invalid form data.");
            }

            try
            {
                foreach (var field in formData.Fields)
                {
                    // Assuming the service method is responsible for determining which DB operation to perform  
                    await _formService.InsertFieldValueAsync(field.TableName, field.FieldName, field.Value);
                }

                return Ok("Form data submitted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}