using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    public partial class AverageTrackCondition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackCondition",
                table: "AverageLaps",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackCondition",
                table: "AverageLaps");
        }
    }
}
