using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ViewModels;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Services
{
    public interface IEntityService
    {
        Task CreateEntityAsync(string tableName);
        Task EditEntityAsync(string oldEntityName, string newEntityName);
        Task DeleteEntityAsync(string entityName);
        Task<List<string>> GetAllEntitiesAsync();
        Task AddColumnToTableAsync(string entityName, List<Column> columns);
        Task EditColumnInTableAsync(string entityName, string oldColumnName, string newColumnName, string newColumnType);
        Task<List<string>> GetAllColumnsAsync(string entityName);
        Task<List<Dictionary<string, object>>> GetAllColumnValuesAsync(string entityName);
        Task<List<object>> GetColumnValuesAsync(string entityName, string columnName);
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

        public async Task CreateEntityAsync(string entityName)
        {
            var columnDefinitions = "Id INT PRIMARY KEY";
            var commandText = $"CREATE TABLE {entityName} ({columnDefinitions})";
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText);
        }

        public async Task DeleteEntityAsync(string entityName)
        {
            var commandText = $"DROP TABLE IF EXISTS {entityName}";
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText);
        }

        public async Task EditEntityAsync(string oldEntityName, string newEntityName)
        {
            var commandText = $"ALTER TABLE {oldEntityName} RENAME TO {newEntityName}";
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText);
        }

        public async Task<List<string>> GetAllEntitiesAsync()
        {
            var tableNames = new List<string>();

            var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using (var connection = _dynamicDbContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return tableNames;
        }

        public async Task AddColumnToTableAsync(string entityName, List<Column> columns)
        {
            var commandText = new StringBuilder($"ALTER TABLE {entityName} ADD ");
            commandText.Append(string.Join(", ", columns.Select(x => $"{x.Name} {x.Type}")));
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText.ToString());
        }

        public async Task EditColumnInTableAsync(string entityName, string oldColumnName, string newColumnName, string newColumnType)
        {
            var commandText = $"ALTER TABLE {entityName} CHANGE {oldColumnName} {newColumnName} {newColumnType}";
            await _dynamicDbContext.ExecuteSqlRawAsync(commandText);
        }

        public async Task<List<string>> GetAllColumnsAsync(string entityName)
        {
            var columnNames = new List<string>();

            var query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @entityName";
            using (var connection = _dynamicDbContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@entityName";
                    parameter.Value = entityName;
                    command.Parameters.Add(parameter);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnNames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return columnNames;
        }
        public async Task<List<Dictionary<string, object>>> GetAllColumnValuesAsync(string entityName)
        {
            var query = $"SELECT * FROM {entityName}";
            var result = await _dynamicDbContext.Set<Dictionary<string, object>>().FromSqlRaw(query).ToListAsync();

            return result;
        }

        public async Task<List<object>> GetColumnValuesAsync(string entityName, string columnName)
        {
            var query = $"SELECT {columnName} FROM {entityName}";
            var result = await _dynamicDbContext.Set<Dictionary<string, object>>().FromSqlRaw(query).ToListAsync();

            return result.Select(row => row[columnName]).ToList();
        }
    }
}