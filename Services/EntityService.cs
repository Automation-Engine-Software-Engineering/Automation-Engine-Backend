using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Services
{
    public interface IEntityService
    {
        Task CreateEntityAsync(int? formId, Entity entity);
        Task RemoveEntityAsync(Entity entity);
        Task UpdateEntityAsync(Entity oldEntity);
        Task<List<Entity>> GetAllEntitiesAsync(int? formId);
        Task<Entity> GetEntitiesByIdAsync(int entityId);
        Task AddColumnToTableAsync(Entity entity, List<Peroperty> columns);
        Task UpdatePeropertyInTableAsync(Peroperty peroperty);
        Task<List<Peroperty>> GetAllColumnsAsync();
        Task<List<Peroperty>> GetAllColumnValuesAsync(int entityId);
        Task<Peroperty> GetColumnValuesAsync(int PeropertyId);
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

        public async Task CreateEntityAsync(int? formId, Entity entity)
        {
            using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                var columnDefinitions = "Id INT PRIMARY KEY";
                command.CommandText = $"CREATE TABLE @TableName ({columnDefinitions})";
                var parameters = new List<SqlParameter>() { new SqlParameter("@TableName", entity.TableName) };
                await _dynamicDbContext.ExecuteSqlRawAsync(command, parameters);
            }
            if (formId != null)
            {
                var form = await _context.Form.FirstOrDefaultAsync(x => x.Id == formId);
                entity.Forms = [form];
            }
            _context.Entity.Add(entity);
        }

        public async Task RemoveEntityAsync(Entity entity)
        {
            using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"DROP TABLE IF EXISTS @TableName";
                var parameters = new List<SqlParameter>() { new SqlParameter("@TableName", entity.TableName) };
                await _dynamicDbContext.ExecuteSqlRawAsync(command, parameters);
            }
            _context.Entity.Remove(entity);
        }

        public async Task UpdateEntityAsync(Entity entity)
        {
            using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"ALTER TABLE @OldTableName RENAME TO @NewTableName";
                var parameters = new List<SqlParameter>() {
                    new SqlParameter("@TableName", entity.TableName) ,  new SqlParameter("@NewTableName", entity.TableName)
                };
                await _dynamicDbContext.ExecuteSqlRawAsync(command, parameters);
            }
            var feachModel = await _context.Entity.FirstOrDefaultAsync(x => x.Id == entity.Id);
            feachModel.PreviewName = entity.PreviewName;
            feachModel.TableName = entity.TableName;
            feachModel.Peroperties = entity.Peroperties;
            _context.Entity.Update(feachModel);
        }

        public async Task<List<Entity>> GetAllEntitiesAsync(int? formId)
        {
            var result = new List<Entity>();
            if (formId != null)
                result = await _context.Entity.Include(x => x.Forms)
                    .Where(x => x.Forms.Any(x => x.Id == formId)).ToListAsync();
            else
                result = await _context.Entity.ToListAsync();
            return result;

        }

        public async Task<Entity> GetEntitiesByIdAsync(int entityId)
        {
            var result = await _context.Entity.Include(x => x.Peroperties)
                .FirstOrDefaultAsync(x => x.Id == entityId);
            return result;
        }

        public async Task AddColumnToTableAsync(Entity entity, List<Peroperty> columns)
        {
            using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"ALTER TABLE @TableName ADD ";
                for (int i = 0; i < columns.Count; i++)
                {
                    command.CommandText += "@Entity" + i + " " + "@Type" + i;
                    if (i != columns.Count-1)
                        command.CommandText += " , ";
                };

                var parameters = new List<SqlParameter>();
                for (int i = 0; i < columns.Count; i++)
                {
                    parameters.Add(new SqlParameter("@Entity"+i, columns[i].PeropertyName));
                    parameters.Add(new SqlParameter("@Type" + i, columns[i].Type));
                    columns[i].EntityId = entity.Id;
                    columns[i].Entity = null;
                }
                parameters.Add(new SqlParameter("@TableName", entity.TableName));
                await _dynamicDbContext.ExecuteSqlRawAsync(command, parameters);
            }
            await _context.Peroperty.AddRangeAsync(columns);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePeropertyInTableAsync(Peroperty peroperty)
        {
            var feachModel = await _context.Peroperty.FirstOrDefaultAsync(x => x.Id == peroperty.Id);
            var table = await _context.Entity.FirstOrDefaultAsync(x => x.Id == feachModel.EntityId);
            using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"ALTER TABLE @TableName ALTER COLUMN @ColumnName @ColumnType;";

                var parameters = new List<SqlParameter>() { new SqlParameter("@TableName", table.TableName),
                    new SqlParameter("@ColumnName", peroperty.PeropertyName),
                    new SqlParameter("@ColumnType", peroperty.Type)};

                await _dynamicDbContext.ExecuteSqlRawAsync(command, parameters);
            }

            var result = await _context.Peroperty.FirstOrDefaultAsync(x => x.Id == peroperty.Id);
            result.PreviewName = peroperty.PreviewName;
            result.PeropertyName = peroperty.PeropertyName;
            result.DefaultValue = peroperty.DefaultValue;
            result.AllowNull = peroperty.AllowNull;
            _context.AddAsync(result);
        }

        public async Task<List<Peroperty>> GetAllColumnsAsync()
        {
            return await _context.Peroperty.ToListAsync();
        }

        public async Task<List<Peroperty>> GetAllColumnValuesAsync(int entityId)
        {
            return await _context.Peroperty.Where(x => x.EntityId == entityId).ToListAsync();
        }

        public async Task<Peroperty> GetColumnValuesAsync(int peropertyId)
        {
            return await _context.Peroperty.FirstOrDefaultAsync(x => x.Id == peropertyId);
        }
        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //public async Task InsertFieldValueAsync(string tableName, string fieldName, string value)
        //{
        //    var tableExists = await TableExistsAsync(tableName);
        //    if (!tableExists)
        //    {
        //        throw new Exception($"Table '{tableName}' does not exist.");
        //    }
        //    var query = $"INSERT INTO {tableName} ({fieldName}) VALUES ({value})";
        //    await _dynamicDbContext.ExecuteSqlRawAsync(query);
        //}

        //private async Task<bool> TableExistsAsync(string tableName)
        //{
        //    var query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
        //    using (var connection = _dynamicDbContext.Database.GetDbConnection())
        //    {
        //        await connection.OpenAsync();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = query;
        //            command.Parameters.Add(new SqlParameter("@TableName", tableName));
        //            var result = await command.ExecuteScalarAsync();
        //            var count = Convert.ToInt32(result);
        //            return count > 0;
        //        }
        //    }
        //}
    }
}