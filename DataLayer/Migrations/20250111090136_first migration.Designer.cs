﻿// <auto-generated />
using DataLayer.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataLayer.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20250111090136_first migration")]
    partial class firstmigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-rc.1.24451.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Entities.Models.FormBuilder.Form", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BackgroundColor")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BackgroundImgPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HtmlFormBody")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAutoHeight")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRepeatedImage")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("SizeHeight")
                        .HasColumnType("float");

                    b.Property<double>("SizeWidth")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.ToTable("Form");
                });

            modelBuilder.Entity("Entities.Models.MainEngine.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "مدیر سیستم",
                            Name = "Admin"
                        });
                });

            modelBuilder.Entity("Entities.Models.MainEngine.Role_User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Role_Users");
                });

            modelBuilder.Entity("Entities.Models.MainEngine.Role_Workflow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("WorkflowId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("WorkflowId");

                    b.ToTable("Role_Workflows");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            RoleId = 1,
                            WorkflowId = 1
                        },
                        new
                        {
                            Id = 2,
                            RoleId = 1,
                            WorkflowId = 2
                        });
                });

            modelBuilder.Entity("Entities.Models.MainEngine.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("IP")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Salt")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserAgent")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Entities.Models.TableBuilder.Entity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PreviewName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Entity");
                });

            modelBuilder.Entity("Entities.Models.TableBuilder.EntityProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DefaultErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DefaultValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EntityId")
                        .HasColumnType("int");

                    b.Property<string>("IconClass")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IsRequiredErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PreviewName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PropertyName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ToolType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("Property");
                });

            modelBuilder.Entity("Entities.Models.Workflows.Node", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("FormId")
                        .HasColumnType("int");

                    b.Property<float>("Height")
                        .HasColumnType("real");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastNodeId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NextNodeId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<float>("Width")
                        .HasColumnType("real");

                    b.Property<int>("WorkflowId")
                        .HasColumnType("int");

                    b.Property<float>("X")
                        .HasColumnType("real");

                    b.Property<float>("Y")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("FormId");

                    b.HasIndex("LastNodeId");

                    b.HasIndex("NextNodeId");

                    b.HasIndex("WorkflowId");

                    b.ToTable("Node");
                });

            modelBuilder.Entity("Entities.Models.Workflows.Workflow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Workflow");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Form Builder",
                            Name = "Form Builder"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Workflow Builder",
                            Name = "Workflow Builder"
                        });
                });

            modelBuilder.Entity("Entities.Models.Workflows.Workflow_User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("WorkflowId")
                        .HasColumnType("int");

                    b.Property<string>("WorkflowState")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("WorkflowId");

                    b.ToTable("Workflow_User");
                });

            modelBuilder.Entity("EntityForm", b =>
                {
                    b.Property<int>("EntitiesId")
                        .HasColumnType("int");

                    b.Property<int>("FormsId")
                        .HasColumnType("int");

                    b.HasKey("EntitiesId", "FormsId");

                    b.HasIndex("FormsId");

                    b.ToTable("EntityForm");
                });

            modelBuilder.Entity("Entities.Models.MainEngine.Role_User", b =>
                {
                    b.HasOne("Entities.Models.MainEngine.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Entities.Models.MainEngine.Role_Workflow", b =>
                {
                    b.HasOne("Entities.Models.MainEngine.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.Workflows.Workflow", "Workflow")
                        .WithMany("Role_Workflows")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("Entities.Models.TableBuilder.EntityProperty", b =>
                {
                    b.HasOne("Entities.Models.TableBuilder.Entity", "Entity")
                        .WithMany("Properties")
                        .HasForeignKey("EntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("Entities.Models.Workflows.Node", b =>
                {
                    b.HasOne("Entities.Models.FormBuilder.Form", "Form")
                        .WithMany()
                        .HasForeignKey("FormId");

                    b.HasOne("Entities.Models.Workflows.Node", "LastNode")
                        .WithMany()
                        .HasForeignKey("LastNodeId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Entities.Models.Workflows.Node", "NextNode")
                        .WithMany()
                        .HasForeignKey("NextNodeId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Entities.Models.Workflows.Workflow", "Workflow")
                        .WithMany("Nodes")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Form");

                    b.Navigation("LastNode");

                    b.Navigation("NextNode");

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("Entities.Models.Workflows.Workflow_User", b =>
                {
                    b.HasOne("Entities.Models.Workflows.Workflow", "Workflow")
                        .WithMany("workflowUser")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("EntityForm", b =>
                {
                    b.HasOne("Entities.Models.TableBuilder.Entity", null)
                        .WithMany()
                        .HasForeignKey("EntitiesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.FormBuilder.Form", null)
                        .WithMany()
                        .HasForeignKey("FormsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Entities.Models.TableBuilder.Entity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("Entities.Models.Workflows.Workflow", b =>
                {
                    b.Navigation("Nodes");

                    b.Navigation("Role_Workflows");

                    b.Navigation("workflowUser");
                });
#pragma warning restore 612, 618
        }
    }
}
