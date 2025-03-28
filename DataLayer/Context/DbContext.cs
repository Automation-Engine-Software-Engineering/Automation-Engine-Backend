using System.Security.Cryptography.X509Certificates;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.MainEngine;
using DataLayer.Models.TableBuilder;
using DataLayer.Models.WorkFlow;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Context
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options) { }
        #region basic database
        public DbSet<Form> Form { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityProperty> Property { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Role_User> Role_Users { get; set; }
        #endregion

        public DbSet<WorkFlow> WorkFlow { get; set; }
        public DbSet<Node> Node { get; set; }
        public DbSet<WorkFlow_User> WorkFlow_User { get; set; }
        public DbSet<Role_WorkFlow> Role_WorkFlows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>()
                 .HasOne(n => n.LastNode)
                 .WithMany()
                 .HasForeignKey(n => n.LastNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.NextNode)
                 .WithMany()
                 .HasForeignKey(n => n.NextNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

                 modelBuilder.Entity<Node>()
                 .HasOne(n => n.WorkFlow)
                 .WithMany(n => n.Nodes)
                 .OnDelete(DeleteBehavior.Cascade);
        }
        
    }
}