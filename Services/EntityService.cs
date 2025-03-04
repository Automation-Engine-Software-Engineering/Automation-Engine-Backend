using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using Tools;
using ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Services
{
    public interface IEntityService
    {
        Task CreateEntityAsync(Entity entity);
        Task CreateEntityAsync(int formId, Entity entity);
        Task RemoveEntityAsync(int entityId);
        Task UpdateEntityAsync(Entity entity);
        Task<List<Entity>> GetAllEntitiesAsync();
        Task<List<Entity>> GetAllEntitiesAsync(int? formId);
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
            if (formId == null) throw new CostumExeption("فرم معتبر نمی باشد");

            var columnDefinitions = "Id INT PRIMARY KEY";
            var CommandText = $"CREATE TABLE @TableName ({columnDefinitions})";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", entity.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
              ?? throw new CostumExeption("فرم یافت نشد.");

            entity.Forms = new List<Form>() { feachModel };

            _context.Entity.Add(entity);
        }                         
        public async Task RemoveEntityAsync(int entityId)
        {
            var result = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entityId)
                 ?? throw new CostumExeption("حدول یافت نشد.");

            await EntityValidation(result);

            var CommandText = "DROP TABLE IF EXISTS @TableName";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", result.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            if (result.Id == null) throw new CostumExeption("جدول معتبر نمی باشد");
            _context.Entity.Remove(result);
        }
        public async Task UpdateEntityAsync(Entity entity)
        {
            await EntityValidation(entity);

            if (entity.Id == null) throw new CostumExeption("جدول معتبر نمی باشد");
            var feachModel = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entity.Id)
                 ?? throw new CostumExeption("حدول یافت نشد.");

            var CommandText = "ALTER TABLE @OldTableName RENAME TO @NewTableName";
            var parameters = new List<(string ParameterName, string ParameterValue)>() { ("@TableName", entity.TableName), ("@NewTableName", entity.TableName) };
            await _dynamicDbContext.ExecuteSqlRawAsync(CommandText, parameters);

            feachModel.PreviewName = entity.PreviewName;
            feachModel.TableName = entity.TableName;
            feachModel.Description = entity.Description;
            feachModel.Properties = entity.Properties;

            _context.Entity.Update(feachModel);
        }

        public async Task<List<Entity>> GetAllEntitiesAsync()
        {
            var result = new List<Entity>();
            result = await _context.Entity.ToListAsync();
            return result;
        }
        public async Task<List<Entity>> GetAllEntitiesAsync(int? formId)
        {
            var result = new List<Entity>();
            if (formId == null) throw new CostumExeption("فرم معتبر نمی باشد");

            var feachModel = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                      ?? throw new CostumExeption("فرم یافت نشد.");

            result = await _context.Entity.Include(x => x.Forms)
                    .Where(x => x.Forms.Any(x => x.Id == feachModel.Id)).ToListAsync();

            return result;

        }

        public async Task<Entity> GetEntitiesByIdAsync(int entityId)
        {
            if (entityId == null) throw new CostumExeption("جدول معتبر نمی باشد");

            var result = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entityId)
                          ?? throw new CostumExeption("حدول یافت نشد.");
            return result;
        }

        public async Task<string> EntityValidation(Entity entity)
        {
            if (entity == null) throw new CostumExeption("اطلاعات جدول معتبر نمی باشد");
            if (entity.PreviewName == null || entity.PreviewName.IsValidateString()) throw new CostumExeption("نام جدول معتبر نمی باشد.");
            if (entity.TableName == null || entity.TableName.IsValidateString()) throw new CostumExeption(".نام جدول معتبر نمی باشد");
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
    }
}