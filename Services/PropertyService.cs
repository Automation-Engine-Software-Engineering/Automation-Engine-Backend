using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;

namespace Services
{
    public interface IPropertyService
    {
        Task AddColumnToTableAsync(EntityProperty property);
        Task UpdateColumnInTableAsync(EntityProperty property);
        Task RemoveColumnByIdAsync(int propertyId);
        Task<ListDto<EntityProperty>> GetAllColumnsAsync(int pageSize, int pageNumber);
        Task<EntityProperty?> GetColumnByIdAsync(int propertyId);
        Task<EntityProperty?> GetColumnByIdIncEntityAsync(int propertyId);
        Task<ListDto<Dictionary<string, object>>> GetColumnValuesByIdAsync(int propertyId, int pageSize, int pageNumber);
        Task<ListDto<EntityProperty>> GetAllColumnByEntityIdAsync(int entityId, int pageSize, int pageNumber);
        Task<ListDto<Dictionary<string, object>>> GetAllColumnValuesByEntityIdAsync(int entityId, int pageSize, int pageNumber);
        ValidationDto<EntityProperty> PropertyValidation(EntityProperty property);
        Task<ValidationDto<string>> SaveChangesAsync();
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
            var entity = await _context.Entity.FirstAsync(x => x.Id == property.EntityId);

            property.DefaultValue.IsValidString();
            //create query             ALTER TABLE string ADD string2 INT  DEFAULT 1  Null
            var commandText = $"ALTER TABLE {entity.TableName} ADD ";
            commandText += property.PropertyName + " " + "@Type ";
            //commandText += "COMMENT" + " " + "@DescriptionValue ";
            commandText += "DEFAULT" + " " + $"'{property.DefaultValue}' ";
            commandText += " NULL";

            var parameters = new List<(string ParameterName, string? ParameterValue)>();
            parameters.Add(("@DescriptionValue", property.Description));

            switch (property.Type)
            {
                case PropertyType.INT:
                    parameters.Add(("@Type", "INT"));
                    break;

                case PropertyType.Float:
                    parameters.Add(("@Type", "Float"));
                    break;
                    
                case PropertyType.Email:
                    parameters.Add(("@Type", "Nvarchar(50)"));
                    break;
                    
                case PropertyType.Color:
                    parameters.Add(("@Type", "Nvarchar(50)"));
                    break;
                    
                case PropertyType.BIT:
                    parameters.Add(("@Type", "BIT"));
                    break;
                    
                case PropertyType.BinaryLong:
                    parameters.Add(("@Type", "binary(max)"));
                    break;
                    
                case PropertyType.NvarcharLong:
                    parameters.Add(("@Type", "Nvarchar(max)"));
                    break;
                    
                case PropertyType.NvarcharShort:
                    parameters.Add(("@Type", "Nvarchar(50)"));
                    break;
                    
                case PropertyType.Password:
                    parameters.Add(("@Type", "Nvarchar(50)"));
                    break;
                    
                case PropertyType.Time:
                    parameters.Add(("@Type", "time(7)"));
                    break;
            }

            //initial action
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText, parameters);
            await _context.Property.AddAsync(property);
        }

        public async Task UpdateColumnInTableAsync(EntityProperty property)
        {
            property.DefaultValue.IsValidString();

            //create query
            var fetchModel = await _context.Property.Include(x => x.Entity).FirstAsync(x => x.Id == property.Id);
            var commandText = $"ALTER TABLE {fetchModel.Entity.TableName} ALTER COLUMN  {fetchModel.PropertyName} @ColumnType  NULL";

            var parameters = new List<(string ParameterName, string ParameterValue)>() {
               ("@DescriptionValue", property.Description) ,
               ("@ColumnType", fetchModel.Type.ToString().Replace("Short", "(50)").Replace("Long", "(max)"))};

            await _dynamicDbContext.ExecuteSqlRawAsync(commandText, parameters);

            //transfer model
            fetchModel.PreviewName = property.PreviewName;
            fetchModel.PropertyName = property.PropertyName;
            fetchModel.DefaultValue = property.DefaultValue;

            _context.Property.Update(fetchModel);
        }

        public async Task RemoveColumnByIdAsync(int propertyId)
        {
            var property = await _context.Property.Include(x => x.Entity).FirstAsync(x => x.Id == propertyId);
            var commandText = $"ALTER TABLE {property.Entity?.TableName} ;";
            commandText += $"DROP COLUMN {property.PropertyName};";
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText, null);

            _context.Property.Remove(property);
        }

        public async Task<ListDto<EntityProperty>> GetAllColumnsAsync(int pageSize, int pageNumber)
        {
            //creat query
            IQueryable<EntityProperty> query = _context.Property;

            //get value and count
            var data = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCount = await query.CountAsync();

            return new ListDto<EntityProperty>(data, totalCount, pageSize, pageNumber);
        }

        public async Task<EntityProperty?> GetColumnByIdAsync(int propertyId)
        {
            var result = await _context.Property.FindAsync(propertyId);
            return result;
        }
        public async Task<EntityProperty?> GetColumnByIdIncEntityAsync(int propertyId)
        {
            var result = await _context.Property.Include(x=>x.Entity).FirstOrDefaultAsync(x => x.Id == propertyId);
            return result;
        }

        public async Task<ListDto<Dictionary<string, object>>> GetColumnValuesByIdAsync(int propertyId, int pageSize, int pageNumber)
        {
            var result = await _context.Property.Include(x => x.Entity).FirstAsync(x => x.Id == propertyId);
            int offset = (pageNumber - 1) * pageSize;
            var commandText = $"SELECT {result.PropertyName} FROM {result.Entity?.TableName} {/*ORDER BY [SomeColumn]*/""} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            commandText = commandText.Replace("@TableName", result.Entity?.TableName);

            return await _dynamicDbContext.ExecuteReaderAsync(commandText);
        }

        public async Task<ListDto<EntityProperty>> GetAllColumnByEntityIdAsync(int entityId, int pageSize, int pageNumber)
        {
            var query = _context.Property.Where(x => x.EntityId == entityId);
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<EntityProperty>(result, count, pageSize, pageNumber);
        }

        public async Task<ListDto<Dictionary<string, object>>> GetAllColumnValuesByEntityIdAsync(int entityId, int pageSize, int pageNumber)
        {
            var query = await _context.Entity.FirstAsync(x => x.Id == entityId);
            int offset = (pageNumber - 1) * pageSize;
            var commandText = $"SELECT * FROM {query.TableName} ORDER BY Id OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return await _dynamicDbContext.ExecuteReaderAsync(commandText);
        }

        public ValidationDto<EntityProperty> PropertyValidation(EntityProperty property)
        {
            if (property == null) return new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", property);
            if (property.EntityId == 0) return new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", property);
            if (property.PreviewName.IsNullOrEmpty() || !property.PreviewName.IsValidString()) return new ValidationDto<EntityProperty>(false, "Property", "CorruptedPropertyPreviewName", property);
            if (property.PropertyName.IsNullOrEmpty() || !property.PropertyName.IsValidStringCommand()) return new ValidationDto<EntityProperty>(false, "Property", "CorruptedPropertyPropertyName", property);
            if (property.Type.GetType() != new PropertyType().GetType()) return new ValidationDto<EntityProperty>(false, "Property", "CorruptedPropertyType", property);
            return new ValidationDto<EntityProperty>(true, "Success", "Success", property);
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
