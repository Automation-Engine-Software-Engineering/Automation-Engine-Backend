using DataLayer.Models.FormBuilder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Context
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options): base(options){}
        public DbSet<Form> Form { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<Peroperty> Peroperty { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {}
    }
}
