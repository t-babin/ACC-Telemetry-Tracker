using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccTelemetryTracker.Datastore.Migrations
{
    public partial class AverageLaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AverageLaps",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageFastestLap = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AverageLaps", x => new { x.CarId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_AverageLaps_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AverageLaps_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AverageLaps_TrackId",
                table: "AverageLaps",
                column: "TrackId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AverageLaps");
        }
    }
}
