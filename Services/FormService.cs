using System.Text.RegularExpressions;
using DataLayer.DbContext;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Tools.TextTools;
using ViewModels.ViewModels.Entity;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task UpdateFormAsync(Form form);
        Task RemoveFormAsync(int formId);
        Task<Form?> GetFormByIdAsync(int formId);
        Task<Form?> GetFormByIdIncEntityAsync(int formId);
        Task<Form?> GetFormByIdIncEntityIncPropertyAsync(int formId);
        Task<ListDto<Form>> GetAllFormsAsync(int pageSize, int pageNumber);
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task<string> GetFormPreviewAsync(Form form, int WorkflowUserId);
        ValidationDto<Form> FormValidation(Form form);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<bool> IsFormExistAsync(int formId);
        Task AddEntitiesToFormAsync(int formId, IEnumerable<int> entityIds);
    }

    public class FormService : IFormService
    {
        private readonly DataLayer.DbContext.Context _context;
        private readonly DynamicDbContext _dynamicDbContext;
        private readonly IHtmlService _htmlService;
        public FormService(DataLayer.DbContext.Context context, DynamicDbContext dynamicDbContext, IHtmlService htmlService)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
            _htmlService = htmlService;
        }

        public async Task CreateFormAsync(Form form)
        {
            await _context.Form.AddAsync(form);
        }

        public async Task UpdateFormAsync(Form form)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == form.Id);

            //transfer model
            fetchModel.Name = form.Name;
            fetchModel.Description = form.Description;

            _context.Form.Update(fetchModel);
        }

        public async Task RemoveFormAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);

            //remove form
            _context.Form.Remove(fetchModel);
        }

        public async Task<Form?> GetFormByIdAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.FindAsync(formId);

            //return model
            return fetchModel;
        }
        public async Task<Form?> GetFormByIdIncEntityAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.Include(x => x.Entities).FirstOrDefaultAsync(x => x.Id == formId);

            //return model
            return fetchModel;
        }
        public async Task<Form?> GetFormByIdIncEntityIncPropertyAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.Include(x => x.Entities).ThenInclude(x => x.Properties).FirstOrDefaultAsync(x => x.Id == formId);

            //return model
            return fetchModel;
        }

        public async Task AddEntitiesToFormAsync(int formId, IEnumerable<int> entityIds)
        {
            // Find the Form
            var form = await _context.Form
                .Include(f => f.Entities)
                .FirstAsync(f => f.Id == formId);

            //var entities = 
            //    .Where(e => entityIds.Contains(e.Id))
            //    .ToListAsync();

            // Add new Entities 
            var newEntities = await _context.Entity
                .Where(e => entityIds.Contains(e.Id))
                .ToListAsync();

            form.Entities?.AddRange(newEntities);

            // Remove Entities 
            if (form.Entities == null)
                form.Entities = new List<Entity>();

            form.Entities = form.Entities?
                .Where(e => entityIds.Contains(e.Id))
                .ToList();
        }
        public async Task<bool> IsFormExistAsync(int formId)
        {
            //check model exist
            var isExist = await _context.Form.AnyAsync(x => x.Id == formId);

            //return model
            return isExist;
        }
        public async Task<ListDto<Form>> GetAllFormsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Form;

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Form>(result, count, pageSize, pageNumber);
        }
        public async Task UpdateFormBodyAsync(int formId, string htmlContent)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);

            //transfer model
            fetchModel.HtmlFormBody = htmlContent;

            await UpdateFormAsync(fetchModel);
        }


        public async Task<string> GetFormPreviewAsync(Form form, int WorkflowUserId)
        {
            if (form.HtmlFormBody == null)
                return "<span>بیتا زر اندیش پارس<span>";

            var htmlBody = form.HtmlFormBody.ToString();
            htmlBody = htmlBody.Replace("disabled", "");
            // htmlBody = htmlBody.Replace("&nbsp;", " ");
            htmlBody = htmlBody.Replace("contenteditable=\"true\"", " ");
            htmlBody = htmlBody.Replace("contenteditable=\"false\"", " ");
            htmlBody = htmlBody.Replace("resize: both;", " ");
            htmlBody = htmlBody.Replace("<tr><td>پیش نمایش</td></tr>", " ");

            //define the preview
            string tagName = "select";
            var attributes = new List<string> { "data-tableid", "data-condition", "data-filter" };
            var tags = _htmlService.FindHtmlTag(htmlBody, tagName, attributes);

            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");
                var condition = _htmlService.GetTagAttributesValue(tag, "data-condition");
                var filter = _htmlService.GetTagAttributesValue(tag, "data-filter");

                var table = await _context.Entity.FirstAsync(x => x.Id == int.Parse(tableId));
                // var query = $"select * from {table.TableName} where" + filter;
                var query = $"select * from [dbo].[{table.TableName}]";
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);

                if (data != null && data.TotalCount != 0)
                {
                    var childTags = new List<string>();
                    var values = _htmlService.GetAttributeConditionValues(condition);

                    if (data.Data != null)
                        foreach (var item in data.Data)
                        {
                            var textValue = condition;
                            foreach (var value in values)
                            {
                                textValue = textValue.Replace("{{" + value + "}}", item.FirstOrDefault(x => x.Key == value).Value.ToString());
                            }

                            var childTag = $"<option value=\"{textValue}\">{textValue}</option>";
                            childTags.Add(childTag);
                        }

                    var newTag = _htmlService.InsertTag(tag, childTags);
                    htmlBody = htmlBody.Replace(tag, newTag);
                }
            }

            tagName = "table";
            attributes = new List<string> { "data-tableid", "data-condition", "data-filter" };
            tags = _htmlService.FindHtmlTag(htmlBody, tagName, attributes);
            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");

                var condition = _htmlService.GetTagAttributesValue(tag, "data-condition");
                condition = condition.Replace("{{", "");
                condition = condition.Replace("}}", "");
                condition = condition.Replace("و", ",");
                condition = condition.Replace("&nbsp;", " ");

                var filter = _htmlService.GetTagAttributesValue(tag, "data-filter");

                var table = await _context.Entity.Include(c => c.Properties).FirstAsync(x => x.Id == int.Parse(tableId));
                var query = $"select " + condition + $" from [dbo].[{table.TableName}]";
                query = query.Replace("&nbsp;", " ");
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);

                if (data != null && data.TotalCount != 0)
                {
                    var childTags = new List<string>();
                    string header = "<tr>";
                    foreach (var item in condition.Split(",").ToList())
                    {
                        var name = table.Properties.FirstOrDefault(x => x.PropertyName == item.Replace(" ", "")).PreviewName;
                        header += $"<th>{name}</th>";
                    }
                    header += "</tr>";
                    childTags.Add(header);

                    if (data.Data != null)
                        foreach (var item in data.Data)
                        {
                            string body = "<tr>";
                            foreach (var item2 in condition.Split(",").ToList())
                            {
                                body += $"<td>{item.GetValueOrDefault(item2.Replace(" ", ""))}</td>";
                            }
                            body += "</tr>";
                            childTags.Add(body);
                        }

                    var newTag = _htmlService.InsertTag(tag, childTags);
                    htmlBody = htmlBody.Replace(tag, newTag);
                }
            }

            //define the edit
            var query = "select ";
            if(){

            }
            return htmlBody;
        }
        public ValidationDto<Form> FormValidation(Form form)
        {
            if (form == null) return new ValidationDto<Form>(false, "Form", "CorruptedForm", form);
            if (form.Name == null || !form.Name.IsValidString()) return new ValidationDto<Form>(false, "Form", "CorruptedFormName", form);
            return new ValidationDto<Form>(true, "Success", "Success", form);
        }
        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "Success", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Form", "CorruptedForm", ex.Message);
            }
        }
    }
}
