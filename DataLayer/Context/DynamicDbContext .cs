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
        public async Task ExecuteSqlRawAsync(DbCommand command, List<SqlParameter>? parameters = null)
        {
            parameters.ForEach(x =>
            {
                x.ToString().IsValidateString();
            });
            using (var connection = Database.GetDbConnection())
            {
                parameters.ForEach(x => command.Parameters.Add(x));
                await connection.OpenAsync();
                command.ExecuteScalarAsync();
                await connection.CloseAsync();
            }
        }

        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, List<SqlParameter>? parameters = null)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                return reader; 
            }
        }
    }
}
