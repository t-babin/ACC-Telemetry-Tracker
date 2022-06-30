﻿// <auto-generated />
using System;
using AccTelemetryTracker.Datastore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    [DbContext(typeof(AccTelemetryTrackerContext))]
    partial class AccTelemetryTrackerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Audit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Log")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MotecId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MotecId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.AverageLap", b =>
                {
                    b.Property<int>("CarId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TrackId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("AverageFastestLap")
                        .HasColumnType("REAL");

                    b.Property<int?>("TrackCondition")
                        .HasColumnType("INTEGER");

                    b.HasKey("CarId", "TrackId");

                    b.HasIndex("TrackId");

                    b.ToTable("AverageLaps", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Class")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cars", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.MotecFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CarId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateInserted")
                        .HasColumnType("TEXT");

                    b.Property<double>("FastestLap")
                        .HasColumnType("REAL");

                    b.Property<string>("FileLocation")
                        .HasColumnType("TEXT");

                    b.Property<int>("NumberOfLaps")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("SessionDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TrackCondition")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TrackId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.HasIndex("TrackId");

                    b.HasIndex("UserId");

                    b.ToTable("MotecFiles", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Track", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tracks", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsValid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Role")
                        .HasColumnType("TEXT");

                    b.Property<string>("ServerName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SignupDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Audit", b =>
                {
                    b.HasOne("AccTelemetryTracker.Datastore.Models.MotecFile", "MotecFile")
                        .WithMany("AuditEvents")
                        .HasForeignKey("MotecId");

                    b.HasOne("AccTelemetryTracker.Datastore.Models.User", "User")
                        .WithMany("AuditEvents")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MotecFile");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.AverageLap", b =>
                {
                    b.HasOne("AccTelemetryTracker.Datastore.Models.Car", "Car")
                        .WithMany("AverageLaps")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AccTelemetryTracker.Datastore.Models.Track", "Track")
                        .WithMany("AverageLaps")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Track");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.MotecFile", b =>
                {
                    b.HasOne("AccTelemetryTracker.Datastore.Models.Car", "Car")
                        .WithMany("MotecFiles")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AccTelemetryTracker.Datastore.Models.Track", "Track")
                        .WithMany("MotecFiles")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AccTelemetryTracker.Datastore.Models.User", "User")
                        .WithMany("MotecFiles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Track");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Car", b =>
                {
                    b.Navigation("AverageLaps");

                    b.Navigation("MotecFiles");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.MotecFile", b =>
                {
                    b.Navigation("AuditEvents");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.Track", b =>
                {
                    b.Navigation("AverageLaps");

                    b.Navigation("MotecFiles");
                });

            modelBuilder.Entity("AccTelemetryTracker.Datastore.Models.User", b =>
                {
                    b.Navigation("AuditEvents");

                    b.Navigation("MotecFiles");
                });
#pragma warning restore 612, 618
        }
    }
}
