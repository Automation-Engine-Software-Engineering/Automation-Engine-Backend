using DataLayer.Models.Enums;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.MainEngine;
using DataLayer.Models.TableBuilder;
using DataLayer.Models.WorkFlows;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace DataLayer.Context
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        #region basic database
        public DbSet<Form> Form { get; set; }

        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityProperty> Property { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Role_User> Role_Users { get; set; }
        #endregion

        public DbSet<WorkFlow> WorkFlow { get; set; }
        public DbSet<Edge> Edge { get; set; }
        public DbSet<Node> Node { get; set; }
        public DbSet<WorkFlow_User> WorkFlow_User { get; set; }
        public DbSet<Role_WorkFlow> Role_WorkFlows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //     modelBuilder.Entity<Entity>().HasData(
            //         new Entity { PreviewName = "کاربران", Description = "اطلاعات اولیه کاربران  (رمز و پسوردها)", TableName = "User" },
            //         new Entity { PreviewName = "نقش ها", Description = "اطلاعات اولیه نفش ها", TableName = "Role" }
            //     );

            //     modelBuilder.Entity<EntityProperty>().HasData(
            //    new EntityProperty { PreviewName = "Id", PropertyName = "Id", AllowNull = false, DefaultValue = null, Type = PropertyTypes.INT, EntityId = 1 },
            //    new EntityProperty { PreviewName = "Name", PropertyName = "Name", AllowNull = false, DefaultValue = null, Type = PropertyTypes.NvarcharLong, EntityId = 1 },
            //    new EntityProperty { PreviewName = "UserName", PropertyName = "UserName", AllowNull = false, DefaultValue = null, Type = PropertyTypes.NvarcharLong, EntityId = 1 },
            //    new EntityProperty { PreviewName = "Password", PropertyName = "Password", AllowNull = false, DefaultValue = null, Type = PropertyTypes.NvarcharLong, EntityId = 1 }
            //     );

            //     modelBuilder.Entity<EntityProperty>().HasData(
            //   new EntityProperty { PreviewName = "Id", PropertyName = "Id", AllowNull = false, DefaultValue = null, Type = PropertyTypes.INT, EntityId = 2 },
            //   new EntityProperty { PreviewName = "Name", PropertyName = "Name", AllowNull = false, DefaultValue = null, Type = PropertyTypes.NvarcharLong, EntityId = 2 },
            //   new EntityProperty { PreviewName = "Description", PropertyName = "Description", AllowNull = false, DefaultValue = null, Type = PropertyTypes.NvarcharLong, EntityId = 1 }
            //    );
            var workFlowSeedData = Enum.GetValues(typeof(WorkFlowEnum))
                .Cast<WorkFlowEnum>()
                .Select(e => new WorkFlow
                {
                    Id = (int)e,
                    Name = e.ToString().InsertSpaces(),
                    Description = e.ToString().InsertSpaces()
                }).ToArray();

            modelBuilder.Entity<WorkFlow>().HasData(workFlowSeedData);
        }
    }
}