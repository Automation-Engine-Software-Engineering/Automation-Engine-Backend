using Entities.Models.Enums;
using Entities.Models.FormBuilder;
using Entities.Models.MainEngine;
using Entities.Models.TableBuilder;
using Entities.Models.WorkFlows;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace DataLayer.DbContext
{
    public class Context : Microsoft.EntityFrameworkCore.DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
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
            var adminRole = new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "مدیر سیستم"
            };
            modelBuilder.Entity<Role>().HasData(adminRole);

            var workFlowSeedData = Enum.GetValues(typeof(WorkFlowEnum))
                .Cast<WorkFlowEnum>()
                .Select(e => new WorkFlow
                {
                    Id = (int)e,
                    Name = e.ToString().InsertSpaces(),
                    Description = e.ToString().InsertSpaces()
                }).ToArray();
            modelBuilder.Entity<WorkFlow>().HasData(workFlowSeedData);

            var roleWorkFlowSeedData = workFlowSeedData.Select(wf => new Role_WorkFlow
            {
                Id = wf.Id,
                WorkFlowId = wf.Id,
                RoleId = adminRole.Id
            }).ToArray();
            modelBuilder.Entity<Role_WorkFlow>().HasData(roleWorkFlowSeedData);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.NextNode)
                 .WithMany()
                 .HasForeignKey(n => n.NextNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.LastNode)
                 .WithMany()
                 .HasForeignKey(n => n.LastNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.WorkFlow)
                 .WithMany(n => n.Nodes)
                 .OnDelete(DeleteBehavior.Cascade);

        }

    }
}