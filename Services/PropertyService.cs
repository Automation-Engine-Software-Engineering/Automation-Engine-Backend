using DataLayer.Context;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Tools;

namespace Services
{
    public interface IPropertyService
    {
        Task AddColumnToTableAsync(EntityProperty property);
        Task UpdatePeropertyInTableAsync(EntityProperty property);
        Task<ListDto<EntityProperty>> GetAllColumnsAsync(int pageSize, int pageNumber);
        Task<EntityProperty?> GetColumnByIdAsync(int propertyId);
        Task<ListDto<Dictionary<string, object>>> GetColumnValuesByIdAsync(int propertyId, int pageSize, int pageNumber);
        Task<ListDto<EntityProperty>> GetAllColumnByEntityIdAsync(int entityId, int pageSize, int pageNumber);
        Task<ListDto<Dictionary<string, object>>> GetColumnValuesByEntityIdAsync(int entityId, int pageSize, int pageNumber);
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

        public async Task AddColumnToTableAsync(EntityProperty property)
        {
            await PropertyValidation(property);

            var CommandText = $"ALTER TABLE @TableName ADD ";
            CommandText += "@Entity" + " " + "@Type";
            CommandText += "COMMENT" + " " + "@DescriptionValue";
            CommandText += "SET DEFAULT" + " " + "@DEFAULTValue";
            CommandText += "NullAble" + " " + "@NullAbleValue";

            var parameters = new List<(string ParameterName, string ParameterValue)>();
            parameters.Add(("@Entity", property.PropertyName));
            parameters.Add(("@Type", property.Type.ToString().Replace("Short", "(50)")
                               .Replace("Long", "(max)")));
            parameters.Add(("@DescriptionValue", property.Description));
            parameters.Add(("@DEFAULTValue", property.DefaultValue));

            if (property.AllowNull)
                parameters.Add(("@NullAbleValue", "Null"));
            else
                parameters.Add(("@NullAbleValue", "NOT NULL"));

            var entity = await _context.Entity.FirstAsync(x => x.Id == property.EntityId);

            parameters.Add(("@TableName", entity.TableName));

            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);
            await _context.Property.AddAsync(property);
        }

        public async Task UpdatePeropertyInTableAsync(EntityProperty property)
        {
            await PropertyValidation(property);

            var fetchModel = await _context.Property.Include(x => x.Entity).FirstAsync(x => x.Id == property.Id);
            var CommandText = "ALTER TABLE @TableName ALTER COLUMN @ColumnName @ColumnType;";
            CommandText += "COMMENT" + " " + "@DescriptionValue";
            CommandText += "DEFAULT" + " " + "@DEFAULTValue";
            CommandText += "NullAble" + " " + "@NullAbleValue";

            var parameters = new List<(string ParameterName, string ParameterValue)>() {
                ("@TableName", fetchModel.Entity?.TableName ?? throw new CustomException()),
               ("@ColumnName", fetchModel.PropertyName) ,
               ("@ColumnType", fetchModel.Type.ToString().Replace("Short", "(50)")
                               .Replace("Long", "(max)"))};

            parameters.Add(("@DescriptionValue", property.Description));
            parameters.Add(("@DEFAULTValue", property.DefaultValue));

            if (property.AllowNull)
                parameters.Add(("@NullAbleValue", "Null"));
            else
                parameters.Add(("@NullAbleValue", "NOT NULL"));


            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            fetchModel.PreviewName = property.PreviewName;
            fetchModel.PropertyName = property.PropertyName;
            fetchModel.DefaultValue = property.DefaultValue;
            fetchModel.AllowNull = property.AllowNull;
            _context.Property.Update(fetchModel);
        }

        public async Task<ListDto<EntityProperty>> GetAllColumnsAsync(int pageSize, int pageNumber)
        {
            IQueryable<EntityProperty> query = _context.Property;
            var data = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCount = await query.CountAsync();
            return new ListDto<EntityProperty>(data, totalCount, pageSize, pageNumber);
        }

        public async Task<EntityProperty?> GetColumnByIdAsync(int propertyId)
        {
            var result = await _context.Property.FirstOrDefaultAsync(x => x.Id == propertyId);
            return result;
        }

        public async Task<ListDto<Dictionary<string, object>>> GetColumnValuesByIdAsync(int propertyId, int pageSize, int pageNumber)
        {
            var result = await _context.Property.Include(x => x.Entity).FirstAsync(x => x.Id == propertyId);
            int offset = (pageNumber - 1) * pageSize;
            var commandText = $"SELECT {result.PropertyName} FROM {result.Entity.TableName} ORDER BY [SomeColumn] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            commandText = commandText.Replace("@TableName", result.Entity.TableName);

            return await _dynamicDbContext.ExecuteReaderAsync(commandText);
        }

        public async Task<ListDto<EntityProperty>> GetAllColumnByEntityIdAsync(int entityId, int pageSize, int pageNumber)
        {
            var result = await _context.Entity.Include(x => x.Properties).FirstAsync(x => x.Id == entityId);
            var count = result.Properties.Count;
            var prresult = result.Properties.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new ListDto<EntityProperty>(prresult, count, pageSize, pageNumber);
        }

        public async Task<ListDto<Dictionary<string, object>>> GetColumnValuesByEntityIdAsync(int entityId, int pageSize, int pageNumber)
        {
            var result = await _context.Entity.FirstAsync(x => x.Id == entityId);
            int offset = (pageNumber - 1) * pageSize;
            var commandText = $"SELECT * FROM {result.TableName} ORDER BY [SomeColumn] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            commandText = commandText.Replace("@TableName", result.TableName);

            return await _dynamicDbContext.ExecuteReaderAsync(commandText);
        }

        public async Task<string> PropertyValidation(EntityProperty property)
        {
            if (property == null) throw new CustomException("اطلاعات عنصر معتبر نمی باشد (نقص در ورود اطلاعات)");
            if (property.EntityId == 0) throw new CustomException("اطلاعات جدول معتبر نمی باشد (از وجود جدول مذکور اطمینان حاصل کنید)");
            if (property.PreviewName.IsNullOrEmpty() || !property.PreviewName.IsValidateString()) throw new CustomException("نام نمایشی عنصر معتبر نمی باشد.");
            if (property.PropertyName.IsNullOrEmpty() || !property.PropertyName.IsValidateStringCommand()) throw new CustomException("نام عنصر معتبر نمی باشد");
            if (property.Type == null || property.Type.GetType() != new PropertyTypes().GetType()) throw new CustomException("نوع عنصر معتبر نمی باشد.");
            if (property.AllowNull == null) throw new CustomException("وضعیت خالی بودن عنصر معتبر نمی باشد");
            return "";
        }

//creat save chenges
        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
        }

    }
}
