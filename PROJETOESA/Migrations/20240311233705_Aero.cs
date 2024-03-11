using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJETOESA.Migrations
{
    /// <inheritdoc />
    public partial class Aero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Trip_TripId",
                table: "Flights");

            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "Flights",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Trip_TripId",
                table: "Flights",
                column: "TripId",
                principalTable: "Trip",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Trip_TripId",
                table: "Flights");

            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "Flights",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Trip_TripId",
                table: "Flights",
                column: "TripId",
                principalTable: "Trip",
                principalColumn: "Id");
        }
    }
}
