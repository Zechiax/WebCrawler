﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebCrawler.Data;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    [DbContext(typeof(CrawlerContext))]
    partial class CrawlerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.7");

            modelBuilder.Entity("TagWebsiteRecord", b =>
                {
                    b.Property<int>("TagsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WebsiteRecordsDataId")
                        .HasColumnType("INTEGER");

                    b.HasKey("TagsId", "WebsiteRecordsDataId");

                    b.HasIndex("WebsiteRecordsDataId");

                    b.ToTable("TagWebsiteRecord");
                });

            modelBuilder.Entity("WebCrawler.Models.CrawlInfoData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("EntryUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("JobId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("Periodicity")
                        .HasColumnType("TEXT");

                    b.Property<string>("RegexPattern")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(".*");

                    b.Property<int>("WebsiteRecordDataId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("WebsiteRecordDataId")
                        .IsUnique();

                    b.ToTable("CrawlInfos");
                });

            modelBuilder.Entity("WebCrawler.Models.Database.WebsiteExecutionData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CrawlInfoDataId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Started")
                        .HasColumnType("TEXT");

                    b.Property<string>("WebsiteGraphSnapshotJson")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CrawlInfoDataId")
                        .IsUnique();

                    b.ToTable("Executions");
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

            modelBuilder.Entity("WebCrawler.Models.WebsiteRecordData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Label")
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

                    b.HasOne("WebCrawler.Models.WebsiteRecordData", null)
                        .WithMany()
                        .HasForeignKey("WebsiteRecordsDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebCrawler.Models.CrawlInfoData", b =>
                {
                    b.HasOne("WebCrawler.Models.WebsiteRecordData", null)
                        .WithOne("CrawlInfoData")
                        .HasForeignKey("WebCrawler.Models.CrawlInfoData", "WebsiteRecordDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebCrawler.Models.Database.WebsiteExecutionData", b =>
                {
                    b.HasOne("WebCrawler.Models.CrawlInfoData", null)
                        .WithOne("LastExecutionData")
                        .HasForeignKey("WebCrawler.Models.Database.WebsiteExecutionData", "CrawlInfoDataId");
                });

            modelBuilder.Entity("WebCrawler.Models.CrawlInfoData", b =>
                {
                    b.Navigation("LastExecutionData");
                });

            modelBuilder.Entity("WebCrawler.Models.WebsiteRecordData", b =>
                {
                    b.Navigation("CrawlInfoData")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
