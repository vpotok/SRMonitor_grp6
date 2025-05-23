﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SRMCore.Data;

#nullable disable

namespace SRMCore.Migrations
{
    [DbContext(typeof(CoreDbContext))]
    partial class CoreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SRMCore.Models.Agent", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AuthToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ComId")
                        .HasColumnType("int");

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.HasKey("Uuid");

                    b.HasIndex("AuthToken")
                        .IsUnique();

                    b.HasIndex("ComId");

                    b.ToTable("Agents");
                });

            modelBuilder.Entity("SRMCore.Models.Company", b =>
                {
                    b.Property<int>("ComId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ComId"));

                    b.Property<string>("ComName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ComId");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("SRMCore.Models.IP", b =>
                {
                    b.Property<int>("ComId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ComId", "IpAddress");

                    b.HasIndex("ComId", "IpAddress")
                        .IsUnique();

                    b.ToTable("IPs");
                });

            modelBuilder.Entity("SRMCore.Models.Log", b =>
                {
                    b.Property<int>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LogId"));

                    b.Property<int>("ComId")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("LogId");

                    b.HasIndex("ComId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("SRMCore.Models.Redmine", b =>
                {
                    b.Property<int>("ComId")
                        .HasColumnType("int");

                    b.Property<string>("ApiKey")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ComId", "ApiKey");

                    b.HasIndex("ComId")
                        .IsUnique();

                    b.HasIndex("ComId", "ApiKey")
                        .IsUnique();

                    b.ToTable("Redmines");
                });

            modelBuilder.Entity("SRMCore.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<int>("ComId")
                        .HasColumnType("int");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.HasIndex("ComId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SRMCore.Models.Agent", b =>
                {
                    b.HasOne("SRMCore.Models.Company", "Company")
                        .WithMany("Agents")
                        .HasForeignKey("ComId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("SRMCore.Models.IP", b =>
                {
                    b.HasOne("SRMCore.Models.Company", "Company")
                        .WithMany("IPs")
                        .HasForeignKey("ComId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("SRMCore.Models.Log", b =>
                {
                    b.HasOne("SRMCore.Models.Company", "Company")
                        .WithMany("Logs")
                        .HasForeignKey("ComId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("SRMCore.Models.Redmine", b =>
                {
                    b.HasOne("SRMCore.Models.Company", "Company")
                        .WithOne("Redmine")
                        .HasForeignKey("SRMCore.Models.Redmine", "ComId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("SRMCore.Models.User", b =>
                {
                    b.HasOne("SRMCore.Models.Company", "Company")
                        .WithMany("Users")
                        .HasForeignKey("ComId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("SRMCore.Models.Company", b =>
                {
                    b.Navigation("Agents");

                    b.Navigation("IPs");

                    b.Navigation("Logs");

                    b.Navigation("Redmine");

                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
