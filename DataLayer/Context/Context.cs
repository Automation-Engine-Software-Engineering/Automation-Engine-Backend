using DataLayer.Models.FormBuilder;
using DataLayer.Models.MainEngine;
using DataLayer.Models.TableBuilder;
using DataLayer.Models.WorkFlow;
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
        public DbSet<EntityProperty> Property { get; set; }
        public DbSet<WorkFlow> WorkFlow { get; set; }
        public DbSet<Edge> Edge { get; set; }
        public DbSet<Node> Node { get; set; }
        public DbSet<WorkFlow_User> WorkFlow_User { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Role_User> Role_Users { get; set; }
        public DbSet<Role_WorkFlow> role_WorkFlows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {}
    }
}
