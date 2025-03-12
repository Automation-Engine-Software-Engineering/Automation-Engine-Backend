using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Tools;

namespace DataLayer.Context
{
    public class DynamicDbContext : DbContext
    {
        public DynamicDbContext(DbContextOptions<DynamicDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { }

        public async Task ExecuteSqlRawAsync(string inputCommand, List<(string ParameterName, string ParameterValue)>? parameters = null)
        {
            parameters.ForEach(x => x.ParameterValue.IsValidateStringCommand());
            using (var connection = Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = inputCommand;

                    // Add parameters to the command
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = param.ParameterName;
                            command.Parameters.Add(parameter);
                        }
                    }

                    await command.ExecuteNonQueryAsync(); // Execute the command
                }
            }
        }

        public async Task<ListDto<Dictionary<string, object>>> ExecuteReaderAsync(string query, List<(string ParameterName, object ParameterValue)>? parameters = null)
        {
            var resultList = new List<Dictionary<string, object>>();

            using (var connection = Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    // Add parameters to the command
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = param.ParameterName;
                            parameter.Value = param.ParameterValue ?? DBNull.Value; // Handle null values
                            command.Parameters.Add(parameter);
                        }
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            resultList.Add(row);
                        }
                    }
                }
            }
            return new ListDto<Dictionary<string, object>>(resultList, resultList.Count, resultList.Count, 1);
        }
    }
}