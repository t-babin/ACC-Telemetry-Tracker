using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    public partial class TrackConditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackCondition",
                table: "MotecFiles",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackCondition",
                table: "MotecFiles");
        }
    }
}
