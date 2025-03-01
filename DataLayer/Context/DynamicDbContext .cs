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
                x.Value.ToString().IsValidateString();
            });

            var query = command.CommandText;
            parameters.ForEach(x =>
            {
              query =  query.Replace(x.ParameterName.ToString() , x.Value.ToString());
            });

            Database.ExecuteSqlRawAsync(query);
            //using (var connection = Database.GetDbConnection())
            //{
            //    command.Parameters.Clear();
            //    parameters.ForEach(x => command.Parameters.Add(x));
            //    await connection.OpenAsync();
            //    command.ExecuteScalarAsync();
            //    await connection.CloseAsync();
            //}

        }

        public async Task<List<Dictionary<string , object>>> ExecuteReaderAsync(DbCommand command, List<SqlParameter>? parameters = null)
        {

            parameters.ForEach(x =>
            {
                x.Value.ToString().IsValidateString();
            });

            var query = command.CommandText;
            parameters.ForEach(x =>
            {
                query = query.Replace(x.ParameterName.ToString(), x.Value.ToString());
            });

            var result = await this.Set<Dictionary<string, object>>().FromSqlRaw(query).ToListAsync();
            return result;
        }
    }
}
