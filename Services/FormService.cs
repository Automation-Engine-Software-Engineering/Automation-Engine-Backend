using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Tools;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task UpdateFormAsync(Form form);
        Task RemoveFormAsync(int formId);
        Task<Form> GetFormAsync(int formId);
        Task<List<Form>> GetAllFormsAsync();
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task GiveTheParameter(List<(int entityId, int propertyId, object value)> data);
        Task SaveChangesAsync();
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
            await FormValidation(form);

            await _context.Form.AddAsync(form);
        }

        public async Task UpdateFormAsync(Form form)
        {
            await FormValidation(form);

            if (form.Id == null) throw new CostumExeption("فرم معتبر نمی باشد");
            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == form.Id)
                   ?? throw new CostumExeption("فرم یافت نشد.");

            feachModel.Name = form.Name;
            feachModel.SizeHeight = form.SizeHeight;
            feachModel.SizeWidth = form.SizeWidth;
            feachModel.BackgroundImgPath = form.BackgroundImgPath;
            feachModel.BackgroundColor = form.BackgroundColor;
            feachModel.Description = form.Description;

            _context.Form.Update(feachModel);
        }

        public async Task RemoveFormAsync(int formId)
        {
            if (formId == null) throw new CostumExeption("فرم معتبر نمی باشد");
            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
             ?? throw new CostumExeption("فرم یافت نشد.");

            _context.Form.Remove(feachModel);
        }

        public async Task<Form> GetFormAsync(int formId)
        {
            if (formId == null) throw new CostumExeption("فرم معتبر نمی باشد");

            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                  ?? throw new CostumExeption("فرم یافت نشد.");

            return feachModel;
        }

        public async Task<List<Form>> GetAllFormsAsync()
        {
            var feachModel = await _context.Form.ToListAsync()
                       ?? throw new CostumExeption("هیچ فرمی یافت نشد.");

            return feachModel;
        }

        public async Task UpdateFormBodyAsync(int formId, string htmlContent)
        {

            if (formId == null) throw new CostumExeption("فرم معتبر نمی باشد");

            if (htmlContent == null) throw new CostumExeption("اطلاعات فرم معتبر نمی باشد");

            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                     ?? throw new CostumExeption("فرم یافت نشد.");

            feachModel.HtmlFormBody = htmlContent;

            await UpdateFormAsync(feachModel);
        }

        public async Task<string> FormValidation(Form form)
        {
            if (form == null) throw new CostumExeption("اطلاعات فرم ناقص می باشد(فرم معتبر نمی باشد(");
            if (form.Name == null || !form.Name.IsValidateString()) throw new CostumExeption("نام فرم معتبر نمی باشد.");
            if (form.SizeWidth == null || form.SizeWidth == 0) throw new CostumExeption(".ابعاد فرم معتبر نمی باشد");
            if (form.SizeHeight == null || form.SizeHeight == 0) throw new CostumExeption("ابعاد فرم معتبر نمی باشد.");
            if (form.BackgroundColor == null) throw new CostumExeption("رنگ فرم معتبر نمی باشد.");
            return "";
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new CostumExeption();
            }
        }

        public async Task GiveTheParameter(List<(int entityId, int propertyId, object value)> data)
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
                var entity = _context.Entity.FirstOrDefaultAsync(xx => xx.Id == x)
                       ?? throw new CostumExeption("موجودیت یافت نشد.");

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
    }
}