using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task UpdateFormAsync(Form form);
        Task RemoveFormAsync(int formId);
        Task<Form?> GetFormByIdAsync(int formId);
        Task<ListDto<Form>> GetAllFormsAsync(int pageSize, int pageNumber);
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task SetTheParameter(List<(int entityId, int propertyId, object value)> data);
        Task<ValidationDto<Form>> FormValidationAsync(Form form);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<bool> IsFormExistAsync(int formId);
    }

    public class FormService : IFormService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;
        public FormService(Context context, DynamicDbContext dynamicDbContext)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
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
            fetchModel.SizeHeight = form.SizeHeight;
            fetchModel.IsAutoHeight = form.IsAutoHeight;
            fetchModel.SizeWidth = form.SizeWidth;
            fetchModel.BackgroundImgPath = form.BackgroundImgPath;
            fetchModel.BackgroundColor = form.BackgroundColor;
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
            var fetchModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId);

            //return model
            return fetchModel;
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
            var count = query.Count();
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

        public async Task SetTheParameter(List<(int entityId, int propertyId, object value)> data)
        {
            var entityIds = new List<int>();
            data.Select(x => x.entityId).ToList().ForEach(x =>
            {
                if (entityIds.Any(xx => xx == x))
                {
                    entityIds.Add(x);
                }
            });

            entityIds.ForEach(x =>
            {
                var entity = _context.Entity.FirstOrDefaultAsync(xx => xx.Id == x);

                var properties = data.Where(x => x.entityId == entity.Id).ToList();
                var propertiesValues = properties.Select(x => _context.Property.FirstOrDefaultAsync(xx => xx.Id == x.propertyId)).ToList();
                var propertiesQuery = "";
                propertiesValues.ForEach(x =>
                {
                    propertiesQuery += x.Result.PropertyName + " ,";
                });
                propertiesQuery += ".";
                propertiesQuery.Replace(",.", "");

                var propertiesQueryValue = "";
                data.Where(x => x.entityId == x.entityId).ToList().ForEach(xx =>
                {
                    propertiesQueryValue += xx.value.ToString() + " ,";
                });
                propertiesQueryValue += ".";
                propertiesQueryValue.Replace(",.", "");

                var query = $"INSERT INTO {entity.Result.TableName} ({propertiesQuery})" +
                $" VALUES ({propertiesQueryValue});";

                _dynamicDbContext.ExecuteSqlRawAsync(query);
            });
        }

        public async Task<ValidationDto<Form>> FormValidationAsync(Form form)
        {
            if (form == null) return new ValidationDto<Form>(false, "Form", "CorruptedForm", form);
            if (form.Name == null || !form.Name.IsValidString()) return new ValidationDto<Form>(false, "Form", "CorruptedFormName", form);
            if (form.SizeWidth == 0) return new ValidationDto<Form>(false, "Form", "CorruptedFormSize", form);
            if (form.SizeHeight == 0 ^ form.IsAutoHeight) return new ValidationDto<Form>(false, "Form", "CorruptedFormSize", form);
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
