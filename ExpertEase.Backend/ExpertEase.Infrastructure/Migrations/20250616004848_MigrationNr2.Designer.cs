﻿// <auto-generated />
using System;
using ExpertEase.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    [DbContext(typeof(WebAppDatabaseContext))]
    [Migration("20250616004848_MigrationNr2")]
    partial class MigrationNr2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "unaccent");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CategorySpecialistProfile", b =>
                {
                    b.Property<Guid>("CategoriesId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SpecialistsUserId")
                        .HasColumnType("uuid");

                    b.HasKey("CategoriesId", "SpecialistsUserId");

                    b.HasIndex("SpecialistsUserId");

                    b.ToTable("SpecialistCategories", (string)null);
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.ContactInfo", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserId");

                    b.ToTable("ContactInfo");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("CancelledAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Currency")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)")
                        .HasDefaultValue("RON");

                    b.Property<DateTime?>("PaidAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ServiceTaskId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StripeAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StripeChargeId")
                        .HasColumnType("text");

                    b.Property<string>("StripePaymentIntentId")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ServiceTaskId")
                        .IsUnique();

                    b.ToTable("Payment");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Photo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsProfilePicture")
                        .HasColumnType("boolean");

                    b.Property<long>("SizeInBytes")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("SpecialistProfileUserId")
                        .HasColumnType("uuid");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SpecialistProfileUserId");

                    b.ToTable("Photo");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Reply", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("RejectedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("RequestId");

                    b.ToTable("Reply");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Request", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ConversationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ReceiverUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("RejectedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RequestedStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SenderUserId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverUserId");

                    b.HasIndex("SenderUserId");

                    b.ToTable("Request");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Review", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<Guid>("ReceiverUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SenderUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ServiceTaskId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverUserId");

                    b.HasIndex("SenderUserId");

                    b.HasIndex("ServiceTaskId")
                        .IsUnique();

                    b.ToTable("Review");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.ServiceTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("CancelledAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("ReplyId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SpecialistId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ReplyId")
                        .IsUnique();

                    b.HasIndex("SpecialistId");

                    b.HasIndex("UserId");

                    b.ToTable("ServiceTask");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.SpecialistProfile", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("StripeAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("YearsExperience")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.ToTable("SpecialistProfile");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AuthProvider")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("ProfilePictureUrl")
                        .HasColumnType("text");

                    b.Property<int>("Rating")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasAlternateKey("Email");

                    b.ToTable("User");
                });

            modelBuilder.Entity("CategorySpecialistProfile", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.SpecialistProfile", null)
                        .WithMany()
                        .HasForeignKey("SpecialistsUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.ContactInfo", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.User", "User")
                        .WithOne("ContactInfo")
                        .HasForeignKey("ExpertEase.Domain.Entities.ContactInfo", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Payment", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.ServiceTask", "ServiceTask")
                        .WithOne("Payment")
                        .HasForeignKey("ExpertEase.Domain.Entities.Payment", "ServiceTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ServiceTask");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Photo", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.SpecialistProfile", null)
                        .WithMany("Portfolio")
                        .HasForeignKey("SpecialistProfileUserId");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Reply", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.Request", "Request")
                        .WithMany("Replies")
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Request");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Request", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.User", "ReceiverUser")
                        .WithMany()
                        .HasForeignKey("ReceiverUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.User", "SenderUser")
                        .WithMany()
                        .HasForeignKey("SenderUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReceiverUser");

                    b.Navigation("SenderUser");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Review", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.User", "ReceiverUser")
                        .WithMany()
                        .HasForeignKey("ReceiverUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.User", "SenderUser")
                        .WithMany()
                        .HasForeignKey("SenderUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.ServiceTask", "ServiceTask")
                        .WithOne("Review")
                        .HasForeignKey("ExpertEase.Domain.Entities.Review", "ServiceTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReceiverUser");

                    b.Navigation("SenderUser");

                    b.Navigation("ServiceTask");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.ServiceTask", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.Reply", "Reply")
                        .WithOne("ServiceTask")
                        .HasForeignKey("ExpertEase.Domain.Entities.ServiceTask", "ReplyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.User", "Specialist")
                        .WithMany()
                        .HasForeignKey("SpecialistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ExpertEase.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reply");

                    b.Navigation("Specialist");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.SpecialistProfile", b =>
                {
                    b.HasOne("ExpertEase.Domain.Entities.User", "User")
                        .WithOne("SpecialistProfile")
                        .HasForeignKey("ExpertEase.Domain.Entities.SpecialistProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Reply", b =>
                {
                    b.Navigation("ServiceTask");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.Request", b =>
                {
                    b.Navigation("Replies");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.ServiceTask", b =>
                {
                    b.Navigation("Payment")
                        .IsRequired();

                    b.Navigation("Review");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.SpecialistProfile", b =>
                {
                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("ExpertEase.Domain.Entities.User", b =>
                {
                    b.Navigation("ContactInfo");

                    b.Navigation("SpecialistProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
