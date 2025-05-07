using System.Text.RegularExpressions;
using DataLayer.DbContext;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
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
        Task<string> GetFormPreviewAsync(Form form, int workflowUserId);
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


        public async Task<string> GetFormPreviewAsync(Form form, int workflowUserId)
        {
            if (form.HtmlFormBody == null)
                return "<span>بیتا زر اندیش پارس<span>";

            var htmlBody = form.HtmlFormBody.ToString();
            htmlBody = htmlBody.Replace("data-disabled=\"true\"", "??>> ");
            htmlBody = htmlBody.Replace("data-readonly=\"true\"", "data-readonly=\"true\" ??>> ");
            htmlBody = htmlBody.Replace("disabled", "");
            // htmlBody = htmlBody.Replace("&nbsp;", " ");
            htmlBody = htmlBody.Replace("contenteditable=\"true\"", " ");
            htmlBody = htmlBody.Replace("contenteditable=\"false\"", " ");
            htmlBody = htmlBody.Replace("resize: both;", " ");
            htmlBody = htmlBody.Replace("??>>", "disabled ");
            htmlBody = htmlBody.Replace("value", "placeholder");

            //define the preview
            string tagName = "select";
            var attributes = new List<string> { "data-tableid", "data-condition", "data-filter" };
            var tags = _htmlService.FindHtmlTag(htmlBody, tagName, attributes);

            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");

                var condition = _htmlService.GetTagAttributesValue(tag, "data-condition");
                var values = _htmlService.GetAttributeConditionValues(condition);
                condition = condition.Replace("&nbsp;", " ");

                var filter = _htmlService.GetTagAttributesValue(tag, "data-filter");
                if (filter != null && filter != "")
                {
                    if (workflowUserId != 0)
                    {
                        var q = _context.Workflow_User.FirstOrDefault(x => x.Id == workflowUserId);
                        var userId = q.UserId;
                        filter = filter.Replace("{{UserId}}", " " + userId + " ");
                        var workflow = q.Workflow.Id;
                        filter = filter.Replace("{{Workflow}}", " " + workflow + " ");
                        var wuIds = _context.Workflow_User.Where(x => x.UserId == userId).ToList();
                        string wuIdString = $"(";

                        int i = 0;
                        wuIds.ForEach(wu =>
                        {
                            if (i != 0)
                                wuIdString += " , ";
                            i++;

                            wuIdString += wu.Id;
                        });
                        wuIdString += ")";
                        filter = filter.Replace("{{Workflow-Users}}", " " + wuIdString + " ");
                    }

                    filter = filter.Replace("{{DateNow}}", " " + DateTime.Now.ToString("yyyy/MM/dd - hh:mm") + " ");
                    filter = filter.Replace("{{", "");
                    filter = filter.Replace("}}", "");
                    filter = filter.Replace("&nbsp;", " ");
                }
                filter = (filter == null || filter == "") ? "1 = 1" : filter;
                var relation = _htmlService.GetTagAttributesValue(tag, "data-relation");
                if (relation != null && relation != "")
                {
                    relation = relation.Replace("{{", "");
                    relation = relation.Replace("}}", "");
                    relation = relation.Replace("و", " ");
                    relation = relation.Replace(",", " ");
                    relation = relation.Replace("&nbsp;", " ");
                }

                var table = await _context.Entity.Include(c => c.Properties).FirstAsync(x => x.Id == int.Parse(tableId));
                var query = $"select [dbo].[{table.TableName}].WorkflowUserId , [dbo].[{table.TableName}].Id";
                foreach (var item in values)
                {
                    query += " , ";
                    query += item + " AS " + item;
                }
                query += $" from [dbo].[{table.TableName}]  {relation} where {filter}";
                query = query.Replace("&nbsp;", " ");

                var data = await _dynamicDbContext.ExecuteReaderAsync(query);

                if (data != null && data.TotalCount != 0)
                {
                    var childTags = new List<string>();
                    childTags.Add($"<option value=\"0-{table.Id}\">{table.PreviewName} را انتخاب کنید</option>");
                    if (data.Data != null)
                        foreach (var item in data.Data)
                        {
                            var textValue = condition;
                            foreach (var value in values)
                            {
                                textValue = textValue.Replace("{{" + value + "}}", item.FirstOrDefault(x => x.Key == value).Value.ToString());
                            }
                            var childTag = $"<option value=\"{item.GetValueOrDefault("Id")}\">{textValue}</option>";

                            var id = _htmlService.GetTagAttributesValue(tag, "id");
                            id = id.Replace("&nbsp;", " ");
                            if (int.Parse(id) == 2 || int.Parse(id) == 3 || int.Parse(id) == 1 || int.Parse(id) == 4)
                                childTag = $"<option value=\"{item.GetValueOrDefault("Id")}-{table.Id}\">{textValue}</option>";

                            childTags.Add(childTag);
                        }

                    var newTag = _htmlService.InsertTagWithRemoveAllChild("select", tag, childTags);
                    htmlBody = htmlBody.Replace(tag, newTag);
                }
                else
                {
                    var childTags = new List<string>();
                    childTags.Add($"<option value=\"0-{table.Id}\">{table.PreviewName} ایی وجود ندارد</option>");

                    var newTag = _htmlService.InsertTagWithRemoveAllChild("select", tag, childTags);
                    htmlBody = htmlBody.Replace(tag, newTag);
                }

                
            }

            tagName = "table";
            attributes = new List<string> { "data-tableid", "data-condition", "data-filter", "data-relation" };
            tags = _htmlService.FindHtmlTag(htmlBody, tagName, attributes);
            foreach (var tag in tags)
            {
                var tableId = _htmlService.GetTagAttributesValue(tag, "data-tableid");

                var condition = _htmlService.GetTagAttributesValue(tag, "data-condition");
                var values = _htmlService.GetAttributeConditionValues(condition);
                condition = condition.Replace("&nbsp;", " ");

                var filter = _htmlService.GetTagAttributesValue(tag, "data-filter");
                if (filter != null && filter != "")
                {
                    if (workflowUserId != 0)
                    {
                        var q = _context.Workflow_User.FirstOrDefault(x => x.Id == workflowUserId);
                        var userId = q.UserId;
                        filter = filter.Replace("{{UserId}}", " " + userId + " ");
                        var workflow = q.Workflow.Id;
                        filter = filter.Replace("{{Workflow}}", " " + workflow + " ");
                        var wuIds = _context.Workflow_User.Where(x => x.UserId == userId).ToList();
                        string wuIdString = $"(";

                        int i = 0;
                        wuIds.ForEach(wu =>
                        {
                            if (i != 0)
                                wuIdString += " , ";
                            i++;

                            wuIdString += wu.Id;
                        });
                        wuIdString += ")";
                        filter = filter.Replace("{{Workflow-User}}", " " + wuIdString + " ");
                    }

                    filter = filter.Replace("{{DateNow}}", " " + DateTime.Now.ToString("yyyy/MM/dd - hh:mm") + " ");
                    filter = filter.Replace("{{", "");
                    filter = filter.Replace("}}", "");
                    filter = filter.Replace("&nbsp;", " ");
                }
                filter = (filter == null || filter == "") ? "1 = 1" : filter;

                var relation = _htmlService.GetTagAttributesValue(tag, "data-relation");

                if (relation != null && relation != "")
                {
                    relation = relation.Replace("{{", "");
                    relation = relation.Replace("}}", "");
                    relation = relation.Replace("و", " ");
                    relation = relation.Replace(",", " ");
                    relation = relation.Replace("&nbsp;", " ");
                }
                var table = await _context.Entity.Include(c => c.Properties).FirstAsync(x => x.Id == int.Parse(tableId));
                var query = $"select [dbo].[{table.TableName}].WorkflowUserId";
                for (int i = 0; i < values.Count; i++)
                {
                    query += " , ";
                    query += values[i] + " AS a" + i;
                }
                query += $" from [dbo].[{table.TableName}]  {relation} where {filter} ";
                query = query.Replace("&nbsp;", " ");
                var data = await _dynamicDbContext.ExecuteReaderAsync(query);

                if (data != null && data.TotalCount != 0)
                {
                    var icon = _htmlService.ExtractContentAfterTableCell(tag);

                    var childTags = new List<string>();
                    var headerCount = values.Count;

                    string header = "<tr style=\"height: 50px;\">";
                    foreach (var item in values)
                    {
                        var name = new EntityProperty();
                        if (item.Split(".").ToList().Count != 1)
                            name = table.Properties.FirstOrDefault(x => x.PropertyName == item.Replace(" ", "").Split('.')[1]);
                        else
                            name = table.Properties.FirstOrDefault(x => x.PropertyName == item.Replace(" ", ""));

                        if (name != null)
                            header += $"<th style=\"width: {90 / headerCount}%;\">{name.PreviewName}</th>";
                        else
                            header += $"<th style=\"width: {90 / headerCount}%;\">بدون نام</th>";

                    }
                    header += "</tr>";
                    childTags.Add(header);

                    if (data.Data != null)
                        foreach (var item in data.Data)
                        {
                            string body = "<tr style=\"height: 50px;\">";
                            for (int i = 0; i < values.Count; i++)
                            {
                                body += $"<td style=\"width: {90 / headerCount}%;\">{item.GetValueOrDefault("a" + i)}</td>";
                            }
                            var newIcon = icon.Replace("data-workflow-user=\"\"", $"data-workflow-user=\"{item.GetValueOrDefault("WorkflowUserId")}\" style=\"width:5%;\"")
                            .Replace("<td", "<td style=\"text-align: center;\"");
                            body += newIcon;
                            body += "</tr>";
                            childTags.Add(body);
                        }

                    var newTag = _htmlService.InsertTagWithRemoveAllChild("table", tag, childTags);
                    htmlBody = htmlBody.Replace(tag, newTag);
                    // htmlBody = htmlBody.Replace("<tr>\n         <td>\n          پیش نمایش جدول\n", "");
                }
            }

            tagName = "input";
            tags = _htmlService.FindSingleHtmlTag(htmlBody);
            var curentTime = DateTime.Now;
            tags.ForEach(x =>
            {
                var replace = x.Replace("placeholder=\"\" =\"\"", $"value=\"{curentTime.ToString("yyyy-MM-dd")}\" disabled");
                htmlBody = htmlBody.Replace(x, replace);
            });


            ////define the edit
            //var query = "select ";
            //if(){

            //}
            if (form.Entities != null)
                form.Entities.ForEach(entity =>
                {
                    var updateQuery = $"SELECT TOP(1) * FROM [dbo].[{entity.TableName}] where WorkflowUserId = {workflowUserId}";
                    var Updatedata = _dynamicDbContext.ExecuteReaderAsync(updateQuery);
                    if (Updatedata.Result.TotalCount != 0)
                    {
                        entity.Properties.ForEach(x =>
                        {
                            var res = Updatedata.Result.Data.FirstOrDefault(xx => xx.Any(n => n.Key == x.PropertyName)).FirstOrDefault(n => n.Key == x.PropertyName).Value.ToString();
                            if (res != null)
                            {
                                htmlBody = htmlBody.Replace($"id=\"{x.Id}\"", $"id=\"{x.Id}\" value=\"{res}\"");
                            }
                        });
                    }
                });

            return htmlBody;
        }

        public async Task SaveFormData(int workflowUserId, List<SaveDataDTO> formData)
        {
            List<Entity> entites = new List<Entity>();
            foreach (var prop in formData)
            {
                var property = await _context.Property.Include(x => x.Entity).FirstOrDefaultAsync(x => x.Id == prop.id);
                if (property == null)
                    throw new CustomException<int>(new ValidationDto<int>(false, "Property", "PropertyNotFound", workflowUserId), 500);

                if (!entites.Any(x => property != null && x == property.Entity && x.Description == prop.group))
                {
                    var entity = property.Entity;
                    entity.Description = prop.group;

                    if (entity == null)
                        throw new CustomException<int>(new ValidationDto<int>(false, "Entity", "EntityNotFound", workflowUserId), 500);

                    entity.Properties = [property];
                    entites.Add(entity);
                }
            }

            var endList = entites.Where(x => x.TableName == "RelationLists").ToList();
            var topList = entites.Where(x => x.TableName != "RelationLists").ToList();
            topList.AddRange(endList);
            await CreatAndExeQuery(workflowUserId, formData, topList);
        }
        private async Task CreatAndExeQuery(int workflowUserId, List<SaveDataDTO> formData, List<Entity> entites)
        {

            foreach (var entity in entites)
            {
                var updateQuery = $"SELECT TOP(1) Id FROM [dbo].[{entity.TableName}]  where WorkflowUserId = {workflowUserId}";
                var Updatedata = await _dynamicDbContext.ExecuteReaderAsync(updateQuery);
                if (Updatedata.TotalCount != 0)
                {
                    var tId = 0;
                    string query = $"Update [dbo].[{entity.TableName}] set ";
                    var propName = new List<string>();
                    var propValue = new List<string>();

                    entity.Properties?.ForEach(x =>
                    {
                        propName.Add(x.PropertyName);
                        propValue.Add(formData.FirstOrDefault(xx => xx.id == x.Id).content.ToString() ?? "");
                    });

                    propValue.ForEach(x => x.IsValidString());
                    int i = 0;
                    propName.ForEach(x =>
                    {
                        if (i != 0)
                            query += " , ";

                        i++;

                        query += " " + x + " =";

                        if (x == "on" || x == "off")
                        {
                            if (x == "on")
                                query += 1;
                            else
                                query += 0;
                        }
                        else
                        {
                            if (entity.TableName == "RelationLists")
                            {
                                tId = int.Parse(propValue[i - 1].Split("-")[1]);
                                query += $"{propValue[i - 1].Split("-")[0]}";
                            }
                            else
                            {
                                query += $"N'{propValue[i - 1]}'";
                            }
                        }
                    });

                    query += $" where WorkflowUserId = {workflowUserId}";
                    if (entity.TableName == "RelationLists")
                    {
                        var rIds = await _context.Entity_EntityRelation.Where(x => ((x.ChildId == tId) || (x.ParentId == tId))).ToListAsync();
                        var resultIds = rIds.Where(x => entites.Any(xx => xx.Id == x.ParentId || xx.Id == x.ChildId)).ToList();

                        string q = "(";
                        resultIds.ForEach(x => q += x.Id);
                        q += ")";
                        query += $" and RelationId IN {q}";
                    }
                    await _dynamicDbContext.ExecuteSqlRawAsync(query);

                }
                else
                {
                    var countQuery = $"SELECT TOP(1) id FROM [dbo].[{entity.TableName}] ORDER BY id DESC";
                    var data = await _dynamicDbContext.ExecuteReaderAsync(countQuery);

                    string query = $"Insert into [dbo].[{entity.TableName}] (";
                    int i = 0;
                    var propName = new List<string>();
                    var propValue = new List<string>();
                    var tId = 0;

                    entity.Properties?.ForEach(x =>
                    {
                        propName.Add(x.PropertyName);
                        propValue.Add(formData.FirstOrDefault(xx => xx.id == x.Id).content.ToString() ?? "");
                    });

                    propValue.ForEach(x => x.IsValidString());
                    i = 0;
                    if (entity.TableName != "User")
                    {
                        query += "Id";
                        query += " , WorkflowUserId";
                    }
                    else
                    {
                        query += " WorkflowUserId";
                    }

                    propName.ForEach(x =>
                    {
                        query += " , ";
                        query += x;
                        i++;
                    });

                    if (entity.TableName == "RelationLists")
                    {
                        if (query.Contains("Element1"))
                        {
                            query += " , Element2 , RelationId";
                        }
                        else
                        {
                            query += " , Element1 , RelationId";
                        }
                    }

                    query += ") Values (";
                    i = 0;
                    var id = 1;

                    if (entity.TableName != "User")
                    {
                        if (data.Data.ToList().Count != 0)
                        {
                            id = int.Parse(data.Data.ToList()[0]["id"].ToString()) + 1;
                        }
                        query += id;
                        query += " , " + workflowUserId;
                    }
                    else
                    {
                        query += workflowUserId;
                    }

                    propValue.ForEach(x =>
                    {
                        query += " , ";

                        if (x == "on" || x == "off")
                        {
                            if (x == "on")
                                query += 1;
                            else
                                query += 0;
                        }
                        else
                        {
                            if (entity.TableName != "RelationLists")
                                query += $"N'{x}'";
                            else
                            {
                                if (entity.TableName != "RelationLists")
                                    query += $"N'{x.Split("-")[0]}'";
                                else
                                {
                                    if (int.Parse(x.Split("-")[0]) != 0)
                                        query += $"{x.Split("-")[0]}";
                                    else
                                    {
                                        query += $"NULL";
                                        tId = int.Parse(x.Split("-")[1]);
                                    }
                                }

                            }
                        }
                        i++;

                    });

                    if (entity.TableName == "RelationLists")
                    {
                        var rIds = await _context.Entity_EntityRelation.Where(x => ((x.ChildId == tId) || (x.ParentId == tId))).ToListAsync();
                        var resultIds = entites.Where(x => rIds.Any(xx => xx.ParentId == x.Id || xx.ChildId == x.Id)).ToList();
                        foreach (var x in resultIds)
                        {
                            var newQuery = query;
                            // var tableName = _context.Entity.FirstOrDefault(x => x.Id == tId);
                            var tableQuery = $"select Id from {x.TableName} where WorkflowUserId = {workflowUserId}";
                            var data2 = await _dynamicDbContext.ExecuteReaderAsync(tableQuery);

                            newQuery += $" , {data2.Data.ToList()[0]["Id"]} , {x.Id}";
                            newQuery += ")";

                            var newQuery2 = $"SET IDENTITY_INSERT RelationLists ON;{newQuery}; SET IDENTITY_INSERT RelationLists OFF;";

                            await _dynamicDbContext.ExecuteSqlRawAsync(newQuery2);
                        }
                        ;

                    }
                    else
                    {
                        query += ")";

                        await _dynamicDbContext.ExecuteSqlRawAsync(query);
                    }



                    var result = await _context.Entity_EntityRelation.Where(x => x.ParentId == entity.Id || x.ChildId == entity.Id).ToListAsync();
                    foreach (var x in result)
                    {
                        if (result != null)
                        {
                            var table = _context.Entity.FirstOrDefault(xx => xx.Id == (x.ParentId == entity.Id ? x.ChildId : x.ParentId));
                            query = $"select WorkflowUserId  , Id from {table.TableName} where WorkflowUserId = {workflowUserId}";
                            var data2 = await _dynamicDbContext.ExecuteReaderAsync(query);

                            if (data2.Data.ToList().Count != 0)
                            {
                                if (!_dynamicDbContext.RelationLists.Any(xx => xx.WorkflowUserId == workflowUserId && xx.RelationId == x.Id))
                                {
                                    await _dynamicDbContext.RelationLists.AddAsync(new RelationList()
                                    {
                                        RelationId = x.Id,
                                        Element1 = x.ParentId == entity.Id ? id : int.Parse(data2.Data.ToList()[0]["Id"].ToString()),
                                        Element2 = x.ChildId == entity.Id ? id : int.Parse(data2.Data.ToList()[0]["Id"].ToString()),
                                        WorkflowUserId = workflowUserId
                                    });
                                    _dynamicDbContext.SaveChanges();
                                }
                                else
                                {
                                    var lastRelation = _dynamicDbContext.RelationLists.FirstOrDefault(xx => xx.WorkflowUserId == workflowUserId && xx.RelationId == x.Id);
                                    lastRelation.Element1 = x.ParentId == entity.Id ? id : int.Parse(data2.Data.ToList()[0]["Id"].ToString());
                                    lastRelation.Element2 = x.ChildId == entity.Id ? id : int.Parse(data2.Data.ToList()[0]["Id"].ToString());
                                    lastRelation.WorkflowUserId = workflowUserId;

                                    _dynamicDbContext.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

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
