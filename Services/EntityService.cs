﻿using DataLayer.DbContext;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Tools.TextTools;
using ViewModels.ViewModels.Workflow;
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
        Task<Entity?> GetEntitiesByIdAsync(int entityId);
        CustomException EntityValidation(Entity entity);
        Task SaveChangesAsync();
        Task<ListDto<IsAccessModel>> GetAllEntityForFormAccess(int FormId, int pageSize, int pageNumber);
        Task ReplaceEntityRolesByFormId(int formId, List<int> entiteIds);
        Task<bool> IsEntityExistAsync(int entityId);
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
            var columnDefinitions = "Id INT PRIMARY KEY , WorkflowUserId INT";
            var CommandText = $"CREATE TABLE @TableName ({columnDefinitions})";
            var parameters = new List<(string ParameterName, string ParameterValue)>();
            parameters.Add(("@TableName", entity.TableName));

            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            //initial action
            await _context.Entity.AddAsync(entity);
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

        public async Task<ListDto<Entity>> GetAllEntitiesAsync(int pageSize, int pageNumber, string search = "", int? formId = null)
        {
            //create query
            var query = _context.Entity.AsQueryable();

            //search on name if is not empty
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.TableName.Contains(search) || x.PreviewName.Contains(search));

            //only get items with this form
            if (formId != null && formId != 0)
                query = query.Include(x => x.Forms).Where(x => x.Forms.Any(x => x.Id == formId));

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Entity>(result, count, pageSize, pageNumber);
        }

        public async Task<ListDto<Entity>> GetAllEntitiesByFormIdAsync(int formId, int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Entity.Include(x => x.Forms).Where(x => x.Forms.Any(x => x.Id == formId));

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Entity>(result, count, pageSize, pageNumber);
        }

        public async Task<Entity?> GetEntitiesByIdAsync(int entityId)
        {
            var result = await _context.Entity.Include(x => x.Properties).FirstOrDefaultAsync(x => x.Id == entityId);
            return result;
        }
        public async Task<bool> IsEntityExistAsync(int entityId)
        {
            var result = await _context.Entity.AnyAsync(x => x.Id == entityId);
            return result;
        }

        public CustomException EntityValidation(Entity entity)
        {
            if (entity == null) return new CustomException("Entity", "CorruptedEntity");
            if (entity.PreviewName == null || !entity.PreviewName.IsValidString()) return new CustomException("Entity", "CorruptedEntityPreviewName", entity);
            if (entity.TableName == null || !entity.TableName.IsValidStringCommand()) return new CustomException("Entity", "CorruptedEntityTableName", entity);
            return new CustomException("Success", "Success");
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ListDto<IsAccessModel>> GetAllEntityForFormAccess(int FormId, int pageSize, int pageNumber)
        {
            var count = await _context.Entity.CountAsync();
            var entites = await _context.Entity.Include(x => x.Forms)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = entites.Select(x => new IsAccessModel() { Id = x.Id, Name = x.PreviewName, IsAccess = x.Forms.Any(x => x.Id == FormId) ? true : false }).ToList();

            var list = new ListDto<IsAccessModel>(result, count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }

        public async Task ReplaceEntityRolesByFormId(int formId, List<int> entiteIds)
        {
            var form = _context.Form.Include(x => x.Entities).FirstOrDefault(x => x.Id == formId);
            form.Entities = new List<Entity>();
            var entites = _context.Entity.Where(x => entiteIds.Any(xx => xx == x.Id)).ToList();
            form.Entities = entites;

            _context.Form.Update(form);
        }
    }
}