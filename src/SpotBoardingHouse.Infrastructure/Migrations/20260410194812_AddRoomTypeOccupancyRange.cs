using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotBoardingHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTypeOccupancyRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxOccupants",
                table: "RoomTypes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MinOccupants",
                table: "RoomTypes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RoomType_MaxOccupants",
                table: "RoomTypes",
                sql: "MaxOccupants >= MinOccupants");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RoomType_MinOccupants",
                table: "RoomTypes",
                sql: "MinOccupants > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RoomType_MaxOccupants",
                table: "RoomTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RoomType_MinOccupants",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "MaxOccupants",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "MinOccupants",
                table: "RoomTypes");
        }
    }
}
