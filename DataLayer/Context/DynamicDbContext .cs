using FrameWork.Model.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using Tools.TextTools;

namespace DataLayer.Context
{
    public class DynamicDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DynamicDbContext(DbContextOptions<DynamicDbContext> options , IConfiguration configuration) : base(options)
        {
            this._configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { }

        public async Task ExecuteSqlRawAsync(string inputCommand, List<(string ParameterName, string ParameterValue)>? parameters = null)
        {
            if (parameters != null)
                parameters.ForEach(x => x.ParameterValue.IsValidStringCommand());
            using (var connection = GetDbConnection())
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
                            //var parameter = command.CreateParameter();

                            //parameter.ParameterName = param.ParameterName;
                            //parameter.DbType = DbType.String;
                            //parameter.Value = param.ParameterValue;
                            //command.Parameters.Add(parameter);

                            command.CommandText = command.CommandText.Replace(param.ParameterName, param.ParameterValue);
                        }
                    }

                    command.CommandText.IsValidateStringQuery();
                    await command.ExecuteNonQueryAsync(); // Execute the command
                }
            }
        }

        public async Task<ListDto<Dictionary<string, object>>> ExecuteReaderAsync(string query, List<(string ParameterName, string ParameterValue)>? parameters = null)
        {
            var resultList = new List<Dictionary<string, object>>();

            using (var connection = GetDbConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

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
                            parameter.Value = param.ParameterValue ; // Handle null values
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

        public DbConnection GetDbConnection()
        {
            // Ensure the connection string is properly set
            var connectionString = _configuration.GetConnectionString("DynamicServer"); // Use your configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not set.");
            }

            // Return a new connection instance
            return new SqlConnection(connectionString);
        }
    }
}