using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
namespace Services
{
    public interface IEntityService
    {
        Task CreateEntityAsync(Entity entity);
        Task CreateEntityAsync(int formId, Entity entity);
        Task RemoveEntityAsync(int entityId);
        Task UpdateEntityAsync(Entity entity);
        Task<ListDto<Entity>> GetAllEntitiesAsync(int pageSize, int pageNumber, string? search = null, int? formId = null);
        Task<ListDto<Entity>> GetAllEntitiesByFormIdAsync(int formId, int pageSize, int pageNumber);
        Task<Entity> GetEntitiesByIdAsync(int entityId);
        Task<ValidationDto<Entity>> EntityValidation(Entity entity);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class EntityService : IEntityService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;

        public EntityService(Context context, DynamicDbContext dynamicDbContext)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
        }

        public async Task CreateEntityAsync(Entity entity)
        {
            //sql query command
            var columnDefinitions = "Id INT PRIMARY KEY";
            var CommandText = $"CREATE TABLE @TableName ({columnDefinitions})";
            var parameters = new List<(string ParameterName, string ParameterValue)>();
            parameters.Add(("@TableName", entity.TableName));

            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            //initial action
            _context.Entity.Add(entity);
        }

        public async Task CreateEntityAsync(int formId, Entity entity)
        {
            //initial action
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);
            entity.Forms = new List<Form>() { fetchModel };
            await CreateEntityAsync(entity);
        }

        public async Task RemoveEntityAsync(int entityId)
        {
            //sql query command
            var result = await _context.Entity.FirstAsync(x => x.Id == entityId);
            var CommandText = "DROP TABLE IF EXISTS @TableName";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", result.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            //initial action
            _context.Entity.Remove(result);
        }

        public async Task UpdateEntityAsync(Entity entity)
        {
            //initialize model
            var fetchModel = await _context.Entity.FirstAsync(x => x.Id == entity.Id);

            //sql query command       

            var commandText = $"EXEC sp_rename @OldTableName , @NewTableName";
          
            var parameters = new List<(string ParameterName, string ParameterValue)>();
            parameters.Add(("@OldTableName", fetchModel.TableName));
            parameters.Add(("@NewTableName", entity.TableName));

            await _dynamicDbContext.ExecuteSqlRawAsync(commandText, parameters);
     

            //transfer model
            fetchModel.PreviewName = entity.PreviewName;
            fetchModel.TableName = entity.TableName;
            fetchModel.Description = entity.Description;

            //initial action
            _context.Entity.Update(fetchModel);
        }

        public async Task<ListDto<Entity>> GetAllEntitiesAsync(int pageSize, int pageNumber,string search = "",int? formId = null)
        {
            //create query
            var query = _context.Entity.AsQueryable();

            //search on name if is not empty
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.TableName.Contains(search) || x.PreviewName.Contains(search));

            //only get items with this form
            if (formId != null && formId != 0)
                query = query.Include(x=>x.Forms).Where(x => x.Forms.Any(x=>x.Id == formId));

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Entity>(result, count, pageSize, pageNumber);
        }

        public async Task<ListDto<Entity>> GetAllEntitiesByFormIdAsync(int formId, int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Entity.Include(x => x.Forms).Where(x => x.Forms.Any(x => x.Id == formId));

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Entity>(result, count, pageSize, pageNumber);
        }

        public async Task<Entity> GetEntitiesByIdAsync(int entityId)
        {
            var result = await _context.Entity.Include(x => x.Properties).FirstOrDefaultAsync(x => x.Id == entityId);
            return result;
        }

        public async Task<ValidationDto<Entity>> EntityValidation(Entity entity)
        {
            if (entity == null) return new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", entity);
            if (entity.PreviewName == null || !entity.PreviewName.IsValidString()) return new ValidationDto<Entity>(false, "Entity", "CorruptedEntityPreviewName", entity);
            if (entity.TableName == null || !entity.TableName.IsValidStringCommand()) return new ValidationDto<Entity>(false, "Entity", "CorruptedEntityTableName", entity);
            return new ValidationDto<Entity>(true, "Success", "Success", entity);
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