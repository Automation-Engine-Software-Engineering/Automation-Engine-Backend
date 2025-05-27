using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tools.TextTools;
using ViewModels.ViewModels.Entity;
using ViewModels.ViewModels.FormBuilder;

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
        Task SaveFormData(int workflowUserId, List<SaveDataDTO> formData);
        Task<string> GetFormPreviewAsync(Form form, int workflowUserId, TableInput tableInput);
        CustomException FormValidation(Form form);
        Task<bool> IsFormExistAsync(int formId);
        Task AddEntitiesToFormAsync(int formId, IEnumerable<int> entityIds);
        Task SaveChangesAsync();
    }

    public class FormService : IFormService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;
        private readonly IHtmlService _htmlService;
        public FormService(Context context, DynamicDbContext dynamicDbContext, IHtmlService htmlService)
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



        public async Task<string> GetFormPreviewAsync(Form form, int workflowUserId, TableInput tableInput)
        {
            if (form.HtmlFormBody == null)
                return "<span>بیتا زر اندیش پارس<span>";

            var htmlBody = CleanHtmlBody(form.HtmlFormBody.ToString());
            htmlBody = await ProcessSelectTags(htmlBody, workflowUserId);
            htmlBody = await ProcessLink(htmlBody, workflowUserId);
            htmlBody = await ProcessTableTags(htmlBody, workflowUserId, tableInput);
            htmlBody = await ProcessInputTags(htmlBody, workflowUserId);
            htmlBody = await PopulateEntityValues(htmlBody, form, workflowUserId);

            return htmlBody;
        }

        private string HiddenInput(string htmlBody)
        {
            var tags = _htmlService.FindSingleHtmlTag(htmlBody, "input", new List<string> { "data-hidden" });
            foreach (var item in tags)
            {
                var element = item.Replace("data-hidden=\"true\"", "data-hidden=\"true\" data-disabled=\"true\"");
                element = element.Replace("type=\"text\"", "type=\"hidden\"");
                element = element.Replace("type=\"checkbox\"", "type=\"hidden\"");
                element = element.Replace("type=\"color\"", "type=\"hidden\"");
                element = element.Replace("type=\"email\"", "type=\"hidden\"");
                element = element.Replace("type=\"number\"", "type=\"hidden\"");
                element = element.Replace("type=\"password\"", "type=\"hidden\"");
                element = element.Replace("type=\"radio\"", "type=\"hidden\"");
                element = element.Replace("type=\"range\"", "type=\"hidden\"");
                element = element.Replace("type=\"tel\"", "type=\"hidden\"");
                element = element.Replace("type=\"time\"", "type=\"hidden\"");
                element = element.Replace("type=\"url\"", "type=\"hidden\"");
                element = element.Replace("type=\"date\"", "type=\"hidden\"");
                element = element.Replace("type=\"text\"", "type=\"hidden\"");
                htmlBody = htmlBody.Replace(item, element);
            }
            return htmlBody;
        }

        private string CleanHtmlBody(string htmlBody)
        {
            Console.WriteLine(htmlBody);
            var htmlb = HiddenInput(htmlBody);
            htmlb = htmlb.Replace("data-disabled=\"true\"", "??>> ")
                           .Replace("data-readonly=\"true\"", "data-readonly=\"true\" ??>> ")
                           .Replace("disabled", "")
                           .Replace("contenteditable=\"true\"", " ")
                           .Replace("contenteditable=\"false\"", " ")
                           .Replace("resize: both;", " ")
                           .Replace("??>>", "disabled ")
                           .Replace("value", "placeholder");
            return htmlb;
        }

        private async Task<string> ProcessSelectTags(string htmlBody, int workflowUserId)
        {
            var tags = _htmlService.FindHtmlTag(htmlBody, "select", new List<string> { "data-tableid", "data-condition", "data-filter" });
            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");
                var condition = _htmlService.GetAttributeConditionValues(CleanCondition(_htmlService.GetTagAttributesValue(tag, "data-condition")));
                var conditionString = condition != null && condition.Any()
                ? string.Join(" , ", condition)
                : string.Empty;
                var filter = await ApplyWorkflowFilters(_htmlService.GetTagAttributesValue(tag, "data-filter"), workflowUserId);
                var relation = CleanRelation(_htmlService.GetTagAttributesValue(tag, "data-relation"));
                var query = GenerateSelectQuery(tableId, conditionString, filter, relation);
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);
                htmlBody = ReplaceSelectTag(htmlBody, tag, data, workflowUserId, tableId, CleanCondition(_htmlService.GetTagAttributesValue(tag, "data-condition")));
            }
            return htmlBody;
        }
        private string CleanCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return "*";
            condition = condition.Replace("&nbsp;", " ");
            return condition.Trim();
        }
        private string CleanRelation(string relation)
        {
            if (string.IsNullOrWhiteSpace(relation))
                return ""; // یا مقدار پیش‌فرضی که مناسب است

            return relation.Trim();
        }

        private string ReplaceSelectTag(string htmlBody, string tag, dynamic data, int workflowUserId, string tableId, string previewOption)
        {
            var options = "";
            if (data.TotalCount == 0)
                options += $"<option value=\"0-0\">موردی وجود ندارد</option>";
            else
                options += $"<option value=\"0-0\">لطفا یک گزینه را انتخاب کنید</option>";

            foreach (var row in data.Data)
            {
                var value = row["Id"];
                string text = previewOption;
                foreach (var v in row)
                {
                    text = text.Replace("{{" + v.Key.ToString() + "}}", v.Value.ToString());
                }
                if (row["WorkflowUserId"] == workflowUserId)
                {
                    var id = _htmlService.GetTagAttributesValue(tag, "id");
                    id = id.Replace("&nbsp;", " ");
                    if (int.Parse(id) == 2 || int.Parse(id) == 3 || int.Parse(id) == 1 || int.Parse(id) == 4)
                        options += $"<option value=\"{row["Id"]}-{tableId}\" selected>{text}</option>";
                    else
                        options += $"<option value=\"{value}\" selected>{text}</option>";
                }
                else
                {
                    var id = _htmlService.GetTagAttributesValue(tag, "id");
                    id = id.Replace("&nbsp;", " ");
                    if (int.Parse(id) == 2 || int.Parse(id) == 3 || int.Parse(id) == 1 || int.Parse(id) == 4)
                        options += $"<option value=\"{row["Id"]}-{tableId}\">{text}</option>";
                    else
                        options += $"<option value=\"{value}\">{text}</option>";
                }
            }

            var newTag = _htmlService.InsertTagWithRemoveAllChild("select", tag, options);

            return htmlBody.Replace(tag, newTag);
        }

        private async Task<string> ProcessTableTags(string htmlBody, int workflowUserId, TableInput tableInput)
        {
            var tags = _htmlService.FindHtmlTag(htmlBody, "table", new List<string> { "data-tableid", "data-condition", "data-filter", "data-relation" });
            foreach (var tag in tags)
            {
                var match = Regex.Match(tag, @"\bid\s*=\s*['""]([^'""]+)['""]");
                var id = match.Groups[1].Value;

                var properyName = "";
                var properyType = new Entities.Models.Enums.PropertyType();
                var SearchValue = "";
                var pageNumber = 1;
                if (tableInput.TablePagination.Count() > 0)
                {
                    pageNumber = !tableInput.TablePagination.Any(x => x.Id == id) ? 1 : tableInput.TablePagination.FirstOrDefault(x => x.Id == id).PageNumber;
                    if (pageNumber <= 0)
                        pageNumber = 1;
                }
                if (tableInput.TableSearches.Count() > 0)
                {
                    var searchItem = !tableInput.TableSearches.Any(x => x.Id == id) ? "" : tableInput.TableSearches.FirstOrDefault(x => x.Id == id).SearchElement;
                    if (searchItem != "")
                    {
                        var propery = _context.Property.FirstOrDefault(x => x.Id == int.Parse(searchItem));
                        properyName = propery.PropertyName;
                        properyType = propery.Type;
                        SearchValue = !tableInput.TableSearches.Any(x => x.Id == id) ? "" : tableInput.TableSearches.FirstOrDefault(x => x.Id == id).SearchValue;
                        if (SearchValue == "")
                        {
                            searchItem = "";
                            properyName = "";
                        }
                    }
                }

                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");
                var condition = _htmlService.GetAttributeConditionValues(CleanCondition(_htmlService.GetTagAttributesValue(tag, "data-condition")));
                var conditionString = condition != null && condition.Any()
                ? string.Join(" , ", condition)
                : string.Empty;
                var filter = await ApplyWorkflowFilters(_htmlService.GetTagAttributesValue(tag, "data-filter"), workflowUserId);
                var relation = CleanRelation(_htmlService.GetTagAttributesValue(tag, "data-relation"));
                var size = int.Parse(_htmlService.GetTagAttributesValue(tag, "data-size"));
                var query = GenerateTableQuery(tableId, conditionString, filter, relation, size, pageNumber, properyName, SearchValue, properyType);

                var conditionTest = _htmlService.GetTagAttributesValue(tag, "data-filter");
                if (conditionTest == "...")
                {
                    query = _htmlService.GetTagAttributesValue(tag, "data-relation").Replace("&nbsp;", "").Replace("\t", "");
                    var q = "";
                    if (properyType == PropertyType.INT)
                        q += properyName != "" ? $" AND {properyName} = {SearchValue}" : "";
                    else if (properyType == PropertyType.NvarcharShort || properyType == PropertyType.NvarcharLong || properyType == PropertyType.Password || properyType == PropertyType.Email || properyType == PropertyType.Time || properyType == PropertyType.TextArea)
                        q += properyName != "" ? $" AND {properyName} Like N'%{SearchValue}%'" : "";

                    query = query.Replace("GROUP", $"{q} GROUP");
                    query += $" OFFSET {(pageNumber - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY";
                }

                int i = 0;
                var TableIds = new List<int>();
                var WorkflowUserOfJoin = _htmlService.ExtractWorkflowUserValues(tag);
                foreach (var element in WorkflowUserOfJoin)
                    if (element != "")
                    {
                        var table = _context.Entity.FirstOrDefault(x => x.Id == int.Parse(element));
                        TableIds.Add(table.Id);
                        query = query.Replace("SELECT", $"SELECT [dbo].[{table.TableName}].WorkflowUserId AS '{i}' ,");
                        i++;
                    }

                var data = await _dynamicDbContext.ExecuteReaderAsync(query);
                htmlBody = ReplaceTableTag(htmlBody, tag, data, condition, tableId , i , TableIds);

                htmlBody = await fillSearchItemSelect(htmlBody, id, int.Parse(tableId), condition);
            }
            return htmlBody;
        }

        private async Task<string> fillSearchItemSelect(string htmlBody, string id, int EntityId, List<string> condition)
        {
            var selectTags = _htmlService.FindHtmlTag(htmlBody, "select", new List<string> { "data-search-id" });
            var selectTag = selectTags.FirstOrDefault(x => _htmlService.GetTagAttributesValue(x, "data-search-id") == id);
            var options = "";
            foreach (var conditionValue in condition)
            {
                var property = _context.Property.FirstOrDefault(x => x.PropertyName == conditionValue);
                options += $"<option value=\"{property.Id}\" selected>{property.PreviewName}</option>";
            }
            var newTag = _htmlService.InsertTagWithRemoveAllChild("select", selectTag, options);
            return htmlBody.Replace(selectTag, newTag);
        }

        private string ReplaceTableTag(string htmlBody, string tag, dynamic data, List<string> condition, string tableId, int WorkflowUserIdIndex , List<int> tableIds)
        {
            var tableRows = "";

            var doc = new HtmlDocument();
            doc.LoadHtml(tag);
            var trTag = doc.DocumentNode.SelectNodes("//tr");
            trTag[0].InnerHtml = trTag[0].InnerHtml.Replace("\n            <td>\n              عنوان جدول\n            </td>\n          ", "");
            trTag[0].InnerHtml = trTag[0].InnerHtml.Replace("<td", "<td style=\"width: 60px; text-align: center;\"");
            trTag[1].InnerHtml = trTag[1].InnerHtml.Replace("\n            <td>\n              متن نمایشی\n            </td>\n          ", "");
            trTag[1].InnerHtml = trTag[1].InnerHtml.Replace("<td", "<td style=\"width: 60px; text-align: center;\"");

            var tableRow = "<tr style=\"height: 50px;\">";
            foreach (var row in condition)
            {
                tableRow += $"<th>{_context.Property.FirstOrDefault(x => x.PropertyName == row).PreviewName}</th>";
            }

            tableRow += trTag[0].InnerHtml;
            tableRow += "</tr>";
            tableRows += tableRow;

            foreach (var row in data.Data)
            {
                tableRow = "<tr style=\"height: 50px;\">";
                foreach (var item in condition)
                {
                    var value = row[item];
                    if (value.ToString().Length > 20)
                    {
                        value = value.ToString().Substring(0, 20) + "...";
                    }
                    tableRow += $"<td title=\"{row[item]}\" style=\"text-align: center;padding: 10px;\">{value}</td>";
                }

 
                var iconRow = trTag[1].InnerHtml.Replace("data-workflow-user=\"\"", $"data-workflow-user=\"{row["WorkflowUserId"]}\"");
                for (var i = 0; i < WorkflowUserIdIndex; i++)
                {
                    iconRow = iconRow.Replace($"data-workflow-user=\"{tableIds[i]}\"", $"data-workflow-user=\"{row[i.ToString()]}\"");
                
                }
                tableRow += iconRow ;
                tableRow += "</tr>";
                tableRows += tableRow;
            }

            var newTag = _htmlService.InsertTagWithRemoveAllChild("table", tag, tableRows);
            return htmlBody.Replace(tag, newTag);
        }


        private async Task<string> ProcessInputTags(string htmlBody, int workflowUserId)
        {
            var tags = _htmlService.FindSingleHtmlTag(htmlBody);
            DateTime currentTime = DateTime.Now;
            PersianCalendar persianCalendar = new PersianCalendar();

            int year = persianCalendar.GetYear(currentTime);
            int month = persianCalendar.GetMonth(currentTime);
            int day = persianCalendar.GetDayOfMonth(currentTime);

            string persianDate = $"{year:0000}-{month:00}-{day:00}";

            tags.ForEach(x =>
            {
                var replace = x.Replace("placeholder=\"\"", $"value=\"{persianDate}\" disabled");
                htmlBody = htmlBody.Replace(x, replace);
            });

            htmlBody = await ApplyInputData(htmlBody, workflowUserId);
            return htmlBody;
        }

        private async Task<string> ApplyInputData(string htmlBody, int workflowUserId)
        {
            var tags = _htmlService.FindSingleHtmlTag(htmlBody, "input", new List<string> { "data-tableid", "data-condition", "data-filter", "data-relation" });
            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");
                var condition = _htmlService.GetAttributeConditionValues(CleanCondition(_htmlService.GetTagAttributesValue(tag, "data-condition")));
                var conditionString = condition != null && condition.Any()
                ? string.Join(" , ", condition)
                : string.Empty;
                var filter = await ApplyWorkflowFilters(_htmlService.GetTagAttributesValue(tag, "data-filter"), workflowUserId);
                var relation = CleanRelation(_htmlService.GetTagAttributesValue(tag, "data-relation"));
                var query = "";
                var conditionTest = _htmlService.GetTagAttributesValue(tag, "data-filter");
                if (conditionTest == "...")
                {
                    query = _htmlService.GetTagAttributesValue(tag, "data-relation").Replace("&nbsp;", "").Replace("\t", "");
                }
                else
                {
                    query = GenerateTableQuery(tableId, conditionString, filter, relation, 1, 1);
                }

                var data = await _dynamicDbContext.ExecuteReaderAsync(query);
                var replace = tag;
                if (data.Data.Count() > 0)
                replace = tag.Replace("input", $"input value=\"{data.Data.ToList()[0].ToList()[1].Value}\"");

                htmlBody = htmlBody.Replace(tag, replace);
            }

            return htmlBody;
        }


        private async Task<string> ProcessLink(string htmlBody, int workflowUserId)
        {
            var tags = _htmlService.FindHtmlTag(htmlBody, "a", ["data-entity", "data-property"]);

            tags.ForEach(async x =>
            {
                var entityId = _htmlService.GetTagAttributesValue(x, "data-entity");
                var property = _htmlService.GetTagAttributesValue(x, "data-property");
                var filter = $"{workflowUserId} IN {{Workflow-Users}}";
                var query = GenerateTableQuery(entityId, property,
                await ApplyWorkflowFilters(filter, workflowUserId), "", 1);
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);

                var replace = x.Replace("'data-property'", $"href = \"{data}\""); ;
                htmlBody = htmlBody.Replace(x, replace);
            });
            return htmlBody;
        }

        private async Task<string> PopulateEntityValues(string htmlBody, Form form, int workflowUserId)
        {
            if (form.Entities == null) return htmlBody;
            foreach (var entity in form.Entities)
            {
                var query = $"SELECT TOP(1) * FROM [dbo].[{entity.TableName}] WHERE WorkflowUserId = {workflowUserId}";
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);
                if (data.TotalCount != 0)
                {
                    foreach (var prop in entity.Properties)
                    {
                        var value = data.Data.FirstOrDefault(x => x.ContainsKey(prop.PropertyName))?.GetValueOrDefault(prop.PropertyName)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            htmlBody = htmlBody.Replace($"id=\"{prop.Id}\"", $"id=\"{prop.Id}\" value=\"{value}\"");
                        }
                    }
                }
            }
            return htmlBody;
        }

        private async Task<string> ApplyWorkflowFilters(string filter, int workflowUserId)
        {
            if (string.IsNullOrEmpty(filter)) return "1 = 1";
            if (workflowUserId == 0) return filter;
            var user = await _context.Workflow_User.FirstOrDefaultAsync(x => x.Id == workflowUserId);
            if (user == null) return filter;
            var userId = user.UserId;
            var workflowId = user.Workflow.Id;
            var workflowUserIds = _context.Workflow_User.Where(x => x.UserId == userId).Select(x => x.Id).ToList();
            return filter.Replace("{{Workflow-Users}}", "(" + string.Join(",", workflowUserIds) + ")")
                         .Replace("{{UserId}}", userId.ToString())
                         .Replace("{{Workflow}}", workflowId.ToString())
                         .Replace("{{DateNow}}", DateTime.Now.ToString("yyyy/MM/dd - hh:mm"))
                         .Replace("{{", "").Replace("}}", "").Replace("&nbsp;", " ");
        }

        private string GenerateSelectQuery(string tableId, string condition, string filter, string relation)
        {
            var table = _context.Entity.Include(c => c.Properties).First(x => x.Id == int.Parse(tableId));
            return $"SELECT [dbo].[{table.TableName}].WorkflowUserId, [dbo].[{table.TableName}].Id, {condition} FROM [dbo].[{table.TableName}] {relation} WHERE {filter}".Replace("&nbsp;", " ");
        }

        private string GenerateTableQuery(string tableId, string condition, string filter, string relation, int size = 10, int pageNumber = 1,
         string searchItem = "", string searchValue = "", PropertyType type = PropertyType.INT)
        {
            var table = _context.Entity.Include(c => c.Properties).First(x => x.Id == int.Parse(tableId));
            var query = $"SELECT [dbo].[{table.TableName}].WorkflowUserId, {condition} ";
            query += $"FROM [dbo].[{table.TableName}] ";
            query += $"{relation} ";
            query += $"WHERE {filter}";
            if (type == PropertyType.INT)
                query += searchItem != "" ? $" AND {searchItem} = {searchValue}" : "";
            else if (type == PropertyType.NvarcharShort || type == PropertyType.NvarcharLong || type == PropertyType.Password || type == PropertyType.Email || type == PropertyType.Time || type == PropertyType.TextArea)
                query += searchItem != "" ? $" AND {searchItem} Like N'%{searchValue}%'" : "";
            query += $" ORDER BY [dbo].[{table.TableName}].Id";
            query += $" OFFSET {(pageNumber - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY";
            query = query.Replace("&nbsp;", " ");
            return query;
        }


        public async Task SaveFormData(int workflowUserId, List<SaveDataDTO> formData)
        {
            List<Entity> entites = new List<Entity>();
            foreach (var prop in formData)
            {
                if (prop.group != "E" && prop.id != null)
                {
                    var property = await _context.Property.Include(x => x.Entity).FirstOrDefaultAsync(x => x.Id == prop.id);
                    if (property == null)
                        throw new CustomException("Property", "PropertyNotFound");

                    if (!entites.Any(x => property != null && x == property.Entity && x.Description == prop.group))
                    {
                        var entity = property.Entity;
                        entity.Description = prop.group;

                    if (entity == null)
                        throw new CustomException("Entity", "EntityNotFound");

                        entity.Properties = [property];
                        entites.Add(entity);
                    }
                }
            }

            entites = entites.OrderBy(x => x.TableName == "RelationLists").ToList();
            await CreatAndExeQuery(workflowUserId, formData, entites);
        }

        private async Task CreatAndExeQuery(int workflowUserId, List<SaveDataDTO> formData, List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var existingRecord = await CheckExistingRecordAsync(entity.TableName, workflowUserId);

                if (existingRecord)
                {
                    await HandleUpdateAsync(entity, workflowUserId, formData, entities);
                }
                else
                {
                    await HandleInsertAsync(entity, workflowUserId, formData, entities);
                }
            }
        }

        private async Task<bool> CheckExistingRecordAsync(string tableName, int workflowUserId)
        {
            var query = $"SELECT TOP(1) Id FROM [dbo].[{tableName}] WHERE WorkflowUserId = {workflowUserId}";
            var result = await _dynamicDbContext.ExecuteReaderAsync(query);
            return result.TotalCount != 0;
        }

        private async Task HandleUpdateAsync(Entity entity, int workflowUserId, List<SaveDataDTO> formData, List<Entity> entities)
        {
            var (propNames, propValues) = ExtractProperties(entity, formData);
            var query = BuildUpdateQuery(entity.TableName, workflowUserId, propNames, propValues);

            if (entity.TableName == "RelationLists")
            {
                query = await AddRelationListConditions(query, workflowUserId, propValues, entity, entities);
            }

            await _dynamicDbContext.ExecuteSqlRawAsync(query);
        }

        private async Task HandleInsertAsync(Entity entity, int workflowUserId, List<SaveDataDTO> formData, List<Entity> entities)
        {
            var (propNames, propValues) = ExtractProperties(entity, formData);
            var (query, newId) = await BuildInsertQuery(entity, workflowUserId, propNames, propValues);

            if (entity.TableName == "RelationLists")
            {
                await HandleRelationListsInsert(query, workflowUserId, entity, propValues, entities);
            }
            else
            {
                query += " )";
                await _dynamicDbContext.ExecuteSqlRawAsync(query);
                await HandleEntityRelations(entity, workflowUserId, newId, entities);
            }
        }

        private (List<string>, List<string>) ExtractProperties(Entity entity, List<SaveDataDTO> formData)
        {
            var propNames = new List<string>();
            var propValues = new List<string>();

            entity.Properties?.ForEach(x =>
            {
                propNames.Add(x.PropertyName);
                propValues.Add(formData.FirstOrDefault(xx => xx.id == x.Id)?.content?.ToString() ?? "");
            });

            propValues.ForEach(v => v.IsValidString());
            return (propNames, propValues);
        }

        private string BuildUpdateQuery(string tableName, int workflowUserId, List<string> propNames, List<string> propValues)
        {
            var query = new StringBuilder($"UPDATE [dbo].[{tableName}] SET ");

            for (int i = 0; i < propNames.Count; i++)
            {
                if (i > 0) query.Append(", ");

                query.Append($"{propNames[i]} = {ProcessValue(propNames[i], propValues[i], tableName)}");
            }

            query.Append($" WHERE WorkflowUserId = {workflowUserId}");
            return query.ToString();
        }

        private async Task<string> AddRelationListConditions(
            string query,
            int workflowUserId,
            List<string> propValues,
            Entity entity,
            List<Entity> entities)
        {
            var tId = int.Parse(propValues[0].Split("-")[1]);
            var relations = await _context.Entity_EntityRelation
                .Where(x => x.ChildId == tId || x.ParentId == tId)
                .ToListAsync();

            var validRelationIds = relations
                .Where(x => entities.Any(e => e.Id == x.ParentId || e.Id == x.ChildId))
                .Select(x => x.Id);

            return $"{query} AND RelationId IN ({string.Join(",", validRelationIds)})";
        }

        private async Task<(string, int)> BuildInsertQuery(Entity entity, int workflowUserId, List<string> propNames, List<string> propValues)
        {
            var query = new StringBuilder($"INSERT INTO [dbo].[{entity.TableName}] (");
            var columns = new List<string>();
            var values = new List<string>();
            var newId = await GetNextId(entity.TableName);

            if (entity.TableName != "User")
            {
                columns.Add("Id");
                values.Add(newId.ToString());
                columns.Add("WorkflowUserId");
                values.Add(workflowUserId.ToString());
            }
            else
            {
                columns.Add("WorkflowUserId");
                values.Add(workflowUserId.ToString());
            }

            for (int i = 0; i < propNames.Count; i++)
            {
                columns.Add(propNames[i]);
                values.Add(ProcessValue(propNames[i], propValues[i], entity.TableName));
            }

            if (entity.TableName == "RelationLists")
            {
                columns.AddRange(new[] { columns.Any(x => x == "Element2") ? "Element1" : "Element2", "RelationId" });
            }

            query.Append(string.Join(", ", columns));
            query.Append(") VALUES (");
            query.Append(string.Join(", ", values));

            return (query.ToString(), newId);
        }

        private async Task<int> GetNextId(string tableName)
        {
            if (tableName == "User") return 0;

            var countQuery = $"SELECT TOP(1) id FROM [dbo].[{tableName}] ORDER BY id DESC";
            var data = await _dynamicDbContext.ExecuteReaderAsync(countQuery);
            return data.Data.Any() ? int.Parse(data.Data.First()["id"].ToString()) + 1 : 1;
        }

        private string ProcessValue(string propName, string value, string tableName)
        {
            if (value == "on") return "1";
            if (value == "off") return "0";

            if (tableName == "RelationLists")
            {
                return value.Split("-")[0] ?? "NULL";
            }

            return $"N'{value.Replace("'", "''")}'";
        }

        private async Task HandleRelationListsInsert(
            string query,
            int workflowUserId,
            Entity entity,
            List<string> propValues,
            List<Entity> entities)
        {
            var tId = int.Parse(propValues[0].Split("-")[1]);
            if (tId == 0) return;

            var relations = await _context.Entity_EntityRelation
                .Where(x => x.ChildId == tId || x.ParentId == tId)
                .ToListAsync();

            var relatedEntities = entities
                .Where(e => relations.Any(r => r.ParentId == e.Id || r.ChildId == e.Id))
                .ToList();

            foreach (var ent in relatedEntities)
            {
                var tableQuery = $"SELECT Id FROM {ent.TableName} WHERE WorkflowUserId = {workflowUserId}";
                var data = await _dynamicDbContext.ExecuteReaderAsync(tableQuery);
                var elementId = data.Data.First()["Id"].ToString();
                var rId = relations.FirstOrDefault(x => x.ParentId == ent.Id || x.ChildId == ent.Id);
                query = query += ($" , {elementId} , {rId.Id} )");

                var fullQuery = $"SET IDENTITY_INSERT RelationLists ON; {query}; SET IDENTITY_INSERT RelationLists OFF;";
                await _dynamicDbContext.ExecuteSqlRawAsync(fullQuery);
            }
        }

        private async Task HandleEntityRelations(
            Entity entity,
            int workflowUserId,
            int newId,
            List<Entity> entities)
        {
            var relations = await _context.Entity_EntityRelation
                .Where(x => x.ParentId == entity.Id || x.ChildId == entity.Id)
                .ToListAsync();

            foreach (var relation in relations)
            {
                var relatedEntityId = relation.ParentId == entity.Id ? relation.ChildId : relation.ParentId;
                var relatedTable = _context.Entity.FirstOrDefault(e => e.Id == relatedEntityId)?.TableName;

                var tableQuery = $"SELECT Id FROM [dbo].[{relatedTable}] WHERE WorkflowUserId = {workflowUserId}";
                var data = await _dynamicDbContext.ExecuteReaderAsync(tableQuery);
                if (data.TotalCount != 0)
                {
                    var relatedId = int.Parse(data.Data.First()["Id"].ToString());

                    var existingRelation = await _dynamicDbContext.RelationLists
                        .FirstOrDefaultAsync(r => r.WorkflowUserId == workflowUserId && r.RelationId == relation.Id);

                    if (existingRelation == null)
                    {
                        await _dynamicDbContext.RelationLists.AddAsync(new RelationList
                        {
                            WorkflowUserId = workflowUserId,
                            RelationId = relation.Id,
                            Element1 = relation.ParentId == entity.Id ? newId : relatedId,
                            Element2 = relation.ChildId == entity.Id ? newId : relatedId
                        });
                    }
                    else
                    {
                        existingRelation.Element1 = relation.ParentId == entity.Id ? newId : relatedId;
                        existingRelation.Element2 = relation.ChildId == entity.Id ? newId : relatedId;
                    }
                }
                await _dynamicDbContext.SaveChangesAsync();
            }
        }


        public CustomException FormValidation(Form form)
        {
            var invalidValidation = new CustomException("Form", "CorruptedForm", form);
            if (form == null) return invalidValidation;
            if (form.Name == null || !form.Name.IsValidString()) return invalidValidation;
            return new CustomException("Success", "Success", form);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
