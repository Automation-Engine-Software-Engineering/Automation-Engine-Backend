using Entities.Models.Enums;
using Entities.Models.FormBuilder;
using Entities.Models.MainEngine;
using Entities.Models.TableBuilder;
using Entities.Models.Workflows;
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

        public DbSet<Workflow> Workflow { get; set; }
        public DbSet<Node> Node { get; set; }
        public DbSet<Workflow_User> Workflow_User { get; set; }
        public DbSet<Role_Workflow> Role_Workflows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminRole = new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "مدیر سیستم"
            };
            modelBuilder.Entity<Role>().HasData(adminRole);

            var workflowSeedData = Enum.GetValues(typeof(WorkflowEnum))
                .Cast<WorkflowEnum>()
                .Select(e => new Workflow
                {
                    Id = (int)e,
                    Name = e.ToString().InsertSpaces(),
                    Description = e.ToString().InsertSpaces()
                }).ToArray();
            modelBuilder.Entity<Workflow>().HasData(workflowSeedData);

            var roleWorkflowSeedData = workflowSeedData.Select(wf => new Role_Workflow
            {
                Id = wf.Id,
                WorkflowId = wf.Id,
                RoleId = adminRole.Id
            }).ToArray();
            modelBuilder.Entity<Role_Workflow>().HasData(roleWorkflowSeedData);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.NextNode)
                 .WithMany()
                 .HasForeignKey(n => n.NextNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.PreviousNode)
                 .WithMany()
                 .HasForeignKey(n => n.PreviousNodeId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Node>()
                 .HasOne(n => n.Workflow)
                 .WithMany(n => n.Nodes)
                 .OnDelete(DeleteBehavior.Cascade);

        }

    }
}