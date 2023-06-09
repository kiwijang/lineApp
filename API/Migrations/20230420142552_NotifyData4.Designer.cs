﻿// <auto-generated />
using System;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace API.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230420142552_NotifyData4")]
    partial class NotifyData4
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("API.Entities.AppUser", b =>
                {
                    b.Property<string>("Sub")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.Property<bool>("isSubscribeNotify")
                        .HasColumnType("INTEGER");

                    b.HasKey("Sub");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("API.Entities.NotifyData", b =>
                {
                    b.Property<long>("DataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessTokenEncrypt")
                        .HasColumnType("TEXT");

                    b.Property<string>("AppUserSub")
                        .HasColumnType("TEXT");

                    b.HasKey("DataId");

                    b.HasIndex("AppUserSub")
                        .IsUnique();

                    b.ToTable("NotifyData");
                });

            modelBuilder.Entity("API.Entities.NotifyHist", b =>
                {
                    b.Property<long>("ContentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthorSubSub")
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("TEXT");

                    b.HasKey("ContentId");

                    b.HasIndex("AuthorSubSub");

                    b.ToTable("NotifyHist");
                });

            modelBuilder.Entity("API.Entities.NotifyData", b =>
                {
                    b.HasOne("API.Entities.AppUser", "UserSub")
                        .WithOne("NotifyData")
                        .HasForeignKey("API.Entities.NotifyData", "AppUserSub");

                    b.Navigation("UserSub");
                });

            modelBuilder.Entity("API.Entities.NotifyHist", b =>
                {
                    b.HasOne("API.Entities.AppUser", "AuthorSub")
                        .WithMany("AuthoredNotify")
                        .HasForeignKey("AuthorSubSub");

                    b.Navigation("AuthorSub");
                });

            modelBuilder.Entity("API.Entities.AppUser", b =>
                {
                    b.Navigation("AuthoredNotify");

                    b.Navigation("NotifyData");
                });
#pragma warning restore 612, 618
        }
    }
}
