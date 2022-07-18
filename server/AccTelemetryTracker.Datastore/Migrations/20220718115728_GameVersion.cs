using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    public partial class GameVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AverageLaps",
                table: "AverageLaps");

            migrationBuilder.AddColumn<string>(
                name: "GameVersion",
                table: "MotecFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrackCondition",
                table: "AverageLaps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AverageLaps",
                table: "AverageLaps",
                columns: new[] { "CarId", "TrackId", "TrackCondition" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AverageLaps",
                table: "AverageLaps");

            migrationBuilder.DropColumn(
                name: "GameVersion",
                table: "MotecFiles");

            migrationBuilder.AlterColumn<int>(
                name: "TrackCondition",
                table: "AverageLaps",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AverageLaps",
                table: "AverageLaps",
                columns: new[] { "CarId", "TrackId" });
        }
    }
}
