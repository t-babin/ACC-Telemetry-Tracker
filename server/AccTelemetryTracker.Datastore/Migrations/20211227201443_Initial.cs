using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Class = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    ServerName = table.Column<string>(type: "TEXT", nullable: true),
                    IsValid = table.Column<bool>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    SignupDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotecFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DateInserted = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NumberOfLaps = table.Column<int>(type: "INTEGER", nullable: false),
                    FastestLap = table.Column<double>(type: "REAL", nullable: false),
                    FileLocation = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotecFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotecFiles_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MotecFiles_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MotecFiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MotecFiles_CarId",
                table: "MotecFiles",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_MotecFiles_TrackId",
                table: "MotecFiles",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MotecFiles_UserId",
                table: "MotecFiles",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotecFiles");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
