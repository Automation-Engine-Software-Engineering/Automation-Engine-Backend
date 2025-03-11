using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Services
{
    public interface IPropertyService
    {
        Task AddColumnToTableAsync(int entityId, EntityProperty property);
        Task UpdatePeropertyInTableAsync(EntityProperty property);
        Task<List<EntityProperty>> GetAllColumnsAsync();
        Task<EntityProperty> GetColumnAsyncById(int propertyId);
        Task<List<EntityProperty>> GetAllColumnAsyncByEntityId(int entityId);
        Task<List<Dictionary<string, object>>> GetColumnValuesAsyncById(int entityId);
        Task<string> PropertyValidation(EntityProperty property);
        Task SaveChangesAsync();
    }
    public class PropertyService : IPropertyService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;

        public PropertyService(Context context, DynamicDbContext dynamicDbContext)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
        }
        public async Task AddColumnToTableAsync(int entityId, EntityProperty property)
        {
            if (entityId == null) throw new CustomExeption("جدول معتبر نمی باشد.");

            await PropertyValidation(property);

            var CommandText = $"ALTER TABLE @TableName ADD ";
            CommandText += "@Entity" + " " + "@Type";

            var parameters = new List<(string ParameterName, string ParameterValue)>();

            parameters.Add(("@Entity", property.PropertyName));

            if (property.Type == PropertyTypes.ShortNVARCHAR)
            {
                parameters.Add(("@Type", "nvarchar(250)"));
            }
            else if (property.Type == PropertyTypes.LongNVARCHAR)
            {
                parameters.Add(("@Type", "nvarchar(500)"));
            }
            else
            {
                parameters.Add(("@Type", property.Type.ToString()));
            }

            var entity = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entityId)
                 ?? throw new CustomExeption("جدول یافت نشد."); ;

            parameters.Add(("@TableName", entity.TableName));
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            property.EntityId = entity.Id;
            property.Entity = null;
            await _context.Property.AddAsync(property);
        }

        public async Task UpdatePeropertyInTableAsync(EntityProperty property)
        {
            await PropertyValidation(property);
            if (property.Id == null) throw new CustomExeption("عنصر معتبر نمی باشد");
            var feachModel = await _context.Property.Include(x => x.Entity).FirstOrDefaultAsync(x => x.Id == property.Id)
                           ?? throw new CustomExeption("عنصر یافت نشد."); ;

            var CommandText = "ALTER TABLE @TableName ALTER COLUMN @ColumnName @ColumnType;";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", feachModel.Entity.TableName),
               ("@ColumnName", feachModel.PropertyName) , ("@ColumnType", feachModel.Type.ToString())};

            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            feachModel.PreviewName = property.PreviewName;
            feachModel.PropertyName = property.PropertyName;
            feachModel.DefaultValue = property.DefaultValue;
            feachModel.AllowNull = property.AllowNull;
            _context.AddAsync(feachModel);
        }

        public async Task<List<EntityProperty>> GetAllColumnsAsync()
        {
            var result = new List<EntityProperty>();
            result = await _context.Property.ToListAsync();
            return result;
        }

        public async Task<EntityProperty> GetColumnAsyncById(int propertyId)
        {
            if (propertyId == null) throw new CustomExeption("عنصر معتبر نمی باشد");

            var result = await _context.Property.FirstOrDefaultAsync(x => x.Id == propertyId)
                          ?? throw new CustomExeption("عنصر یافت نشد.");

            return await _context.Property.FirstOrDefaultAsync(x => x.EntityId == propertyId);
        }

        public async Task<List<EntityProperty>> GetAllColumnAsyncByEntityId(int entityId)
        {
            if (entityId == null) throw new CustomExeption("جدول معتبر نمی باشد");

            var result = await _context.Entity.Include(x => x.Properties).FirstOrDefaultAsync(x => x.Id == entityId)
                          ?? throw new CustomExeption("جدول یافت نشد.");

            return result.Properties.ToList();
        }

        public async Task<List<Dictionary<string, object>>> GetColumnValuesAsyncById(int entityId)
        {
            if (entityId == null) throw new CustomExeption("جدول معتبر نمی باشد");

            var result = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entityId)
                          ?? throw new CustomExeption("جدول یافت نشد.");

            var CommandText = "select * from @TableName";
            CommandText = CommandText.Replace("@TableName", result.TableName);

            return await _dynamicDbContext.ExecuteReaderAsync(CommandText);
        }

        public async Task<string> PropertyValidation(EntityProperty property)
        {
            if (property == null) throw new CustomExeption("اطلاعات عنصر معتبر نمی باشد");
            if (property.PreviewName == null || !property.PreviewName.IsValidateString()) throw new CustomExeption("نام عنصر معتبر نمی باشد.");
            if (property.PropertyName == null || !property.PropertyName.IsValidateString()) throw new CustomExeption(".نام عنصر معتبر نمی باشد");
            if (property.Type == null) throw new CustomExeption("نوع عنصر معتبر نمی باشد.");
            if (property.AllowNull == null) throw new CustomExeption(".وضعیت خالی بودن عنصر معتبر نمی باشد");
            if (property.SizeWidth == null || property.SizeWidth == 0) throw new CustomExeption("ابعاد عنصر معتبر نمی باشد.");
            if (property.SizeHeight == null || property.SizeHeight == 0) throw new CustomExeption(".ابعاد عنصر معتبر نمی باشد");
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
                throw new CustomExeption(ex);
            }
        }
    }
}
