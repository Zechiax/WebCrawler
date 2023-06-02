﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebCrawler.Data;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    [DbContext(typeof(CrawlerContext))]
    [Migration("20230531182750_addJobId")]
    partial class addJobId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("TagWebsiteRecord", b =>
                {
                    b.Property<int>("TagsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WebsiteRecordsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("TagsId", "WebsiteRecordsId");

                    b.HasIndex("WebsiteRecordsId");

                    b.ToTable("TagWebsiteRecord");
                });

            modelBuilder.Entity("WebCrawler.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("WebCrawler.Models.WebsiteExecution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdjacencyListJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Started")
                        .HasColumnType("TEXT");

                    b.Property<int>("WebsiteRecordId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("WebsiteRecordId")
                        .IsUnique();

                    b.ToTable("Executions");
                });

            modelBuilder.Entity("WebCrawler.Models.WebsiteRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("JobId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Periodicity")
                        .HasColumnType("TEXT");

                    b.Property<string>("Regex")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WebsiteRecord");
                });

            modelBuilder.Entity("TagWebsiteRecord", b =>
                {
                    b.HasOne("WebCrawler.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebCrawler.Models.WebsiteRecord", null)
                        .WithMany()
                        .HasForeignKey("WebsiteRecordsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebCrawler.Models.WebsiteExecution", b =>
                {
                    b.HasOne("WebCrawler.Models.WebsiteRecord", null)
                        .WithOne("LastExecution")
                        .HasForeignKey("WebCrawler.Models.WebsiteExecution", "WebsiteRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebCrawler.Models.WebsiteRecord", b =>
                {
                    b.Navigation("LastExecution");
                });
#pragma warning restore 612, 618
        }
    }
}
