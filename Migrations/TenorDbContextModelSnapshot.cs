﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tenor.Data;

#nullable disable

namespace Tenor.Migrations
{
    [DbContext(typeof(TenorDbContext))]
    partial class TenorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Tenor.Models.AccessLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ActionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResponseMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("AccessLogs");
                });

            modelBuilder.Entity("Tenor.Models.Counter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Aggregation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ColumnName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("RefColumnName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SubsetId")
                        .HasColumnType("int");

                    b.Property<string>("SupplierId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SubsetId");

                    b.ToTable("Counters");
                });

            modelBuilder.Entity("Tenor.Models.CounterField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FieldId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("FieldId");

                    b.ToTable("CounterFields");
                });

            modelBuilder.Entity("Tenor.Models.CounterFieldValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CounterFieldId")
                        .HasColumnType("int");

                    b.Property<int>("CounterId")
                        .HasColumnType("int");

                    b.Property<string>("FieldValue")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CounterFieldId");

                    b.HasIndex("CounterId");

                    b.ToTable("CounterFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Dashboard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Dashboards");
                });

            modelBuilder.Entity("Tenor.Models.DashboardField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FieldId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("FieldId");

                    b.ToTable("DashboardFields");
                });

            modelBuilder.Entity("Tenor.Models.DashboardFieldValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DashboardFieldId")
                        .HasColumnType("int");

                    b.Property<int>("DashboardId")
                        .HasColumnType("int");

                    b.Property<string>("FieldValue")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DashboardFieldId");

                    b.HasIndex("DashboardId");

                    b.ToTable("DashboardFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<string>("SupplierId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Tenor.Models.ExtraField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ExtraFields");
                });

            modelBuilder.Entity("Tenor.Models.Function", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ArgumentsCount")
                        .HasColumnType("int");

                    b.Property<bool>("IsBool")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Functions");
                });

            modelBuilder.Entity("Tenor.Models.GroupTenantRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("TenantId");

                    b.ToTable("GroupTenantRoles");
                });

            modelBuilder.Entity("Tenor.Models.Kpi", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("OperationId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OperationId");

                    b.ToTable("Kpis");
                });

            modelBuilder.Entity("Tenor.Models.KpiField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FieldId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("FieldId");

                    b.ToTable("KpiFields");
                });

            modelBuilder.Entity("Tenor.Models.KpiFieldValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FieldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("KpiFieldId")
                        .HasColumnType("int");

                    b.Property<int>("KpiId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("KpiFieldId");

                    b.HasIndex("KpiId");

                    b.ToTable("KpiFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Operation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Aggregation")
                        .HasColumnType("int");

                    b.Property<int?>("CounterId")
                        .HasColumnType("int");

                    b.Property<int?>("FunctionId")
                        .HasColumnType("int");

                    b.Property<int?>("KpiId")
                        .HasColumnType("int");

                    b.Property<int?>("OperatorId")
                        .HasColumnType("int");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CounterId");

                    b.HasIndex("FunctionId");

                    b.HasIndex("KpiId");

                    b.HasIndex("OperatorId");

                    b.HasIndex("ParentId");

                    b.ToTable("Operations");
                });

            modelBuilder.Entity("Tenor.Models.Operator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Operators");
                });

            modelBuilder.Entity("Tenor.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Tenor.Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Tenor.Models.ReportField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FieldId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("FieldId");

                    b.ToTable("ReportFields");
                });

            modelBuilder.Entity("Tenor.Models.ReportFieldValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FieldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReportFieldId")
                        .HasColumnType("int");

                    b.Property<int>("ReportId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReportFieldId");

                    b.HasIndex("ReportId");

                    b.ToTable("ReportFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Tenor.Models.RolePermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("PermissionId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("Tenor.Models.Subset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DataTS")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DbLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DeviceId")
                        .HasColumnType("int");

                    b.Property<string>("IndexTS")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLoad")
                        .HasColumnType("bit");

                    b.Property<int?>("MaxDataDate")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("RefDbLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefSchema")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefTableName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SchemaName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("Subsets");
                });

            modelBuilder.Entity("Tenor.Models.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Tenor.Models.TenantDevice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DeviceId")
                        .HasColumnType("int");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("TenantId");

                    b.ToTable("TenantDevice");
                });

            modelBuilder.Entity("Tenor.Models.UserTenantRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("TenantId");

                    b.ToTable("UserTenantRoles");
                });

            modelBuilder.Entity("Tenor.Models.UserToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RefreshTokenExpired")
                        .HasColumnType("datetime2");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TokenExpired")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("Tenor.Models.Counter", b =>
                {
                    b.HasOne("Tenor.Models.Subset", "Subset")
                        .WithMany("Counters")
                        .HasForeignKey("SubsetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subset");
                });

            modelBuilder.Entity("Tenor.Models.CounterField", b =>
                {
                    b.HasOne("Tenor.Models.ExtraField", "ExtraField")
                        .WithMany("CounterFields")
                        .HasForeignKey("FieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExtraField");
                });

            modelBuilder.Entity("Tenor.Models.CounterFieldValue", b =>
                {
                    b.HasOne("Tenor.Models.CounterField", "CounterField")
                        .WithMany("CounterFieldValues")
                        .HasForeignKey("CounterFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Counter", "Counter")
                        .WithMany("CounterFieldValues")
                        .HasForeignKey("CounterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Counter");

                    b.Navigation("CounterField");
                });

            modelBuilder.Entity("Tenor.Models.DashboardField", b =>
                {
                    b.HasOne("Tenor.Models.ExtraField", "ExtraField")
                        .WithMany("DashboardFields")
                        .HasForeignKey("FieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExtraField");
                });

            modelBuilder.Entity("Tenor.Models.DashboardFieldValue", b =>
                {
                    b.HasOne("Tenor.Models.DashboardField", "DashboardField")
                        .WithMany("DashboardFieldValues")
                        .HasForeignKey("DashboardFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Dashboard", "Dashboard")
                        .WithMany("DashboardFieldValues")
                        .HasForeignKey("DashboardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Dashboard");

                    b.Navigation("DashboardField");
                });

            modelBuilder.Entity("Tenor.Models.Device", b =>
                {
                    b.HasOne("Tenor.Models.Device", "Parent")
                        .WithMany("Childs")
                        .HasForeignKey("ParentId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Tenor.Models.GroupTenantRole", b =>
                {
                    b.HasOne("Tenor.Models.Role", "Role")
                        .WithMany("GroupTenantRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Tenant", "Tenant")
                        .WithMany("GroupTenantRoles")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Tenor.Models.Kpi", b =>
                {
                    b.HasOne("Tenor.Models.Operation", "Operation")
                        .WithMany()
                        .HasForeignKey("OperationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Operation");
                });

            modelBuilder.Entity("Tenor.Models.KpiField", b =>
                {
                    b.HasOne("Tenor.Models.ExtraField", "ExtraField")
                        .WithMany("KpiFields")
                        .HasForeignKey("FieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExtraField");
                });

            modelBuilder.Entity("Tenor.Models.KpiFieldValue", b =>
                {
                    b.HasOne("Tenor.Models.KpiField", "KpiField")
                        .WithMany("KpiFieldValues")
                        .HasForeignKey("KpiFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Kpi", "Kpi")
                        .WithMany("KpiFieldValues")
                        .HasForeignKey("KpiId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Kpi");

                    b.Navigation("KpiField");
                });

            modelBuilder.Entity("Tenor.Models.Operation", b =>
                {
                    b.HasOne("Tenor.Models.Counter", "Counter")
                        .WithMany("Operations")
                        .HasForeignKey("CounterId");

                    b.HasOne("Tenor.Models.Function", "Function")
                        .WithMany("Operations")
                        .HasForeignKey("FunctionId");

                    b.HasOne("Tenor.Models.Kpi", "Kpi")
                        .WithMany()
                        .HasForeignKey("KpiId");

                    b.HasOne("Tenor.Models.Operator", "Operator")
                        .WithMany("Operations")
                        .HasForeignKey("OperatorId");

                    b.HasOne("Tenor.Models.Operation", "Parent")
                        .WithMany("Childs")
                        .HasForeignKey("ParentId");

                    b.Navigation("Counter");

                    b.Navigation("Function");

                    b.Navigation("Kpi");

                    b.Navigation("Operator");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Tenor.Models.ReportField", b =>
                {
                    b.HasOne("Tenor.Models.ExtraField", "ExtraField")
                        .WithMany("ReportFields")
                        .HasForeignKey("FieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExtraField");
                });

            modelBuilder.Entity("Tenor.Models.ReportFieldValue", b =>
                {
                    b.HasOne("Tenor.Models.ReportField", "ReportField")
                        .WithMany("ReportFieldValues")
                        .HasForeignKey("ReportFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Report", "Report")
                        .WithMany("ReportFieldValues")
                        .HasForeignKey("ReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Report");

                    b.Navigation("ReportField");
                });

            modelBuilder.Entity("Tenor.Models.RolePermission", b =>
                {
                    b.HasOne("Tenor.Models.Permission", "Permission")
                        .WithMany("RolePermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Role", "Role")
                        .WithMany("RolePermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Tenor.Models.Subset", b =>
                {
                    b.HasOne("Tenor.Models.Device", "Device")
                        .WithMany("Subsets")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("Tenor.Models.TenantDevice", b =>
                {
                    b.HasOne("Tenor.Models.Device", "Device")
                        .WithMany("TenantDevices")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Tenant", "Tenant")
                        .WithMany("TenantDevices")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Tenor.Models.UserTenantRole", b =>
                {
                    b.HasOne("Tenor.Models.Role", "Role")
                        .WithMany("UserTenantRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tenor.Models.Tenant", "Tenant")
                        .WithMany("UserTenantRoles")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Tenor.Models.Counter", b =>
                {
                    b.Navigation("CounterFieldValues");

                    b.Navigation("Operations");
                });

            modelBuilder.Entity("Tenor.Models.CounterField", b =>
                {
                    b.Navigation("CounterFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Dashboard", b =>
                {
                    b.Navigation("DashboardFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.DashboardField", b =>
                {
                    b.Navigation("DashboardFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Device", b =>
                {
                    b.Navigation("Childs");

                    b.Navigation("Subsets");

                    b.Navigation("TenantDevices");
                });

            modelBuilder.Entity("Tenor.Models.ExtraField", b =>
                {
                    b.Navigation("CounterFields");

                    b.Navigation("DashboardFields");

                    b.Navigation("KpiFields");

                    b.Navigation("ReportFields");
                });

            modelBuilder.Entity("Tenor.Models.Function", b =>
                {
                    b.Navigation("Operations");
                });

            modelBuilder.Entity("Tenor.Models.Kpi", b =>
                {
                    b.Navigation("KpiFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.KpiField", b =>
                {
                    b.Navigation("KpiFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Operation", b =>
                {
                    b.Navigation("Childs");
                });

            modelBuilder.Entity("Tenor.Models.Operator", b =>
                {
                    b.Navigation("Operations");
                });

            modelBuilder.Entity("Tenor.Models.Permission", b =>
                {
                    b.Navigation("RolePermissions");
                });

            modelBuilder.Entity("Tenor.Models.Report", b =>
                {
                    b.Navigation("ReportFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.ReportField", b =>
                {
                    b.Navigation("ReportFieldValues");
                });

            modelBuilder.Entity("Tenor.Models.Role", b =>
                {
                    b.Navigation("GroupTenantRoles");

                    b.Navigation("RolePermissions");

                    b.Navigation("UserTenantRoles");
                });

            modelBuilder.Entity("Tenor.Models.Subset", b =>
                {
                    b.Navigation("Counters");
                });

            modelBuilder.Entity("Tenor.Models.Tenant", b =>
                {
                    b.Navigation("GroupTenantRoles");

                    b.Navigation("TenantDevices");

                    b.Navigation("UserTenantRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
