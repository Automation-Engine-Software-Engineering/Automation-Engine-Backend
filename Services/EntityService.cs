using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Tools;
namespace Services
{
    public interface IEntityService
    {
        Task CreateEntityAsync(Entity entity);
        Task CreateEntityAsync(int formId, Entity entity);
        Task RemoveEntityAsync(int entityId);
        Task UpdateEntityAsync(Entity entity);
        Task<ListDto<Entity>> GetAllEntitiesAsync(int pageSize, int pageNumber);
        Task<ListDto<Entity>> GetAllEntitiesAsync(int? formId, int pageSize, int pageNumber);
        Task<Entity> GetEntitiesByIdAsync(int entityId);
        Task<string> EntityValidation(Entity entity);
        Task SaveChangesAsync();
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
            await EntityValidation(entity);

            var columnDefinitions = "Id INT PRIMARY KEY";
            var CommandText = $"CREATE TABLE @TableName ({columnDefinitions})";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", entity.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            _context.Entity.Add(entity);
        }

        public async Task CreateEntityAsync(int formId, Entity entity)
        {
            await EntityValidation(entity);
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);
            entity.Forms = new List<Form>() { fetchModel };
            await CreateEntityAsync(entity);
        }

        public async Task RemoveEntityAsync(int entityId)
        {
            var result = await _context.Entity.FirstAsync(x => x.Id == entityId);
            await EntityValidation(result);

            var CommandText = "DROP TABLE IF EXISTS @TableName";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", result.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            _context.Entity.Remove(result);
        }

        public async Task UpdateEntityAsync(Entity entity)
        {
            await EntityValidation(entity);

            var feachModel = await _context.Entity.FirstAsync(x => x.Id == entity.Id);
            var CommandText = "ALTER TABLE @OldTableName RENAME TO @NewTableName";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", entity.TableName), ("@NewTableName", entity.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            feachModel.PreviewName = entity.PreviewName;
            feachModel.TableName = entity.TableName;
            feachModel.Description = entity.Description;
            feachModel.Properties = entity.Properties;

            _context.Entity.Update(feachModel);
        }

        public async Task<ListDto<Entity>> GetAllEntitiesAsync(int pageSize, int pageNumber)
        {
            var query = _context.Entity;
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Entity>(result, count, pageSize, pageNumber);
        }

        public async Task<ListDto<Entity>> GetAllEntitiesAsync(int? formId, int pageSize, int pageNumber)
        {
            var result = await _context.Form.Include(x => x.Entities).FirstAsync(x => x.Id == formId);
            var entResult = result.Entities.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new ListDto<Entity>(entResult, result.Entities.Count, pageSize, pageNumber);

        }

        public async Task<Entity> GetEntitiesByIdAsync(int entityId)
        {
            var result = await _context.Entity.Include(x => x.Properties).FirstAsync(x => x.Id == entityId);
            return result;
        }

        public async Task<string> EntityValidation(Entity entity)
        {
            if (entity == null) throw new CustomException("اطلاعات جدول معتبر نمی باشد");
            if (entity.PreviewName == null || !entity.PreviewName.IsValidateString()) throw new CustomException("نام جدول معتبر نمی باشد.");
            if (entity.TableName == null || !entity.TableName.IsValidateString()) throw new CustomException(".نام جدول معتبر نمی باشد");
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
                throw new CustomException();
            }
        }
    }
}