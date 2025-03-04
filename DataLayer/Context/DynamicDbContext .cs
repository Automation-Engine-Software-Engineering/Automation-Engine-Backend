using DataLayer.Models.FormBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tools;
using System.Data.Common;

namespace DataLayer.Context
{
    public class DynamicDbContext : DbContext
    {
        public DynamicDbContext(DbContextOptions<DynamicDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { }
        public async Task ExecuteSqlRawAsync(string command, List<(string ParameterName, string ParameterValue)>? parameters = null)
        {
            parameters.ForEach(x =>
            {
                x.ParameterValue.ToString().IsValidateString();
            });

            var query = command;
            parameters.ForEach(x =>
            {
                query = query.Replace(x.ParameterName.ToString(), x.ParameterValue.ToString());
            });

            Database.ExecuteSqlRawAsync(query);
        }

        public async Task<List<Dictionary<string, object>>> ExecuteReaderAsync(string Query)
        {
            var resultList = new List<Dictionary<string, object>>();
            using (var connecction = this.Database.GetDbConnection())
            {
                connecction.OpenAsync();
                var command = connecction.CreateCommand();
                command.CommandText = Query;
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
            return resultList;
        }
    }
}
