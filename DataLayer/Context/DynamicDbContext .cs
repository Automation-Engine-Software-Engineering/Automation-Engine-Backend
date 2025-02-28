using DataLayer.Models.FormBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Context
{
    public class DynamicDbContext : DbContext
    {
        public DynamicDbContext(DbContextOptions<DynamicDbContext> options): base(options){}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {}
        public async Task<int> ExecuteSqlRawAsync(string commandText)
        {
          return  await Database.ExecuteSqlRawAsync(commandText);
        }

        public async Task<int> ExecuteSqlRawAsyncWithParametrs(string commandText , List<SqlParameter> parameters)
        {
          return  await Database.ExecuteSqlRawAsync(commandText , parameters);
        }
    }
}
