using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task UpdateFormAsync(Form form);
        Task RemoveFormAsync(Form form);
        Task<Form> GetFormAsync(int formId);
        Task<List<Form>> GetAllFormsAsync();
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task SaveChangesAsync();
    }

    public class FormService : IFormService
    {
        private readonly Context _context;

        public FormService(Context context)
        {
            _context = context;
        }

        public async Task CreateFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            await _context.Form.AddAsync(form);
        }

        public async Task UpdateFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            var feachModel =await _context.Form.FirstOrDefaultAsync(x => x.Id == form.Id)
                   ?? throw new KeyNotFoundException($"Form not found");

            feachModel.Name = form.Name;
            feachModel.SizeHeight = form.SizeHeight;
            feachModel.SizeWidth = form.SizeWidth;
            feachModel.BackgroundImgPath = form.BackgroundImgPath;
            feachModel.Description = form.Description;
            _context.Form.Update(feachModel);
        }

        public async Task RemoveFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            _context.Form.Remove(form);
        }

        public async Task<Form> GetFormAsync(int formId)
        {
            if (formId == null) throw new ArgumentNullException(nameof(formId));
            return await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                   ?? throw new KeyNotFoundException($"Form not found");
        }

        public async Task<List<Form>> GetAllFormsAsync()
        {
            return await _context.Form.ToListAsync()
                   ?? throw new KeyNotFoundException($"Forms not found");
        }

        public async Task UpdateFormBodyAsync(int formId, string htmlContent)
        {

            if (formId == null) throw new ArgumentNullException(nameof(formId));
            if (htmlContent == null) throw new ArgumentNullException(nameof(htmlContent));
            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                   ?? throw new KeyNotFoundException($"Form not found");

            feachModel.HtmlFormBody = htmlContent;
            await UpdateFormAsync(feachModel);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}