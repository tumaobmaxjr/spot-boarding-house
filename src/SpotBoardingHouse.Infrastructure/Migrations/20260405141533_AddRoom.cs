using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotBoardingHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardingHouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseCapacity = table.Column<int>(type: "int", nullable: false),
                    OccupiedSlots = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AdditionalDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.CheckConstraint("CK_Room_BaseCapacity", "BaseCapacity > 0");
                    table.CheckConstraint("CK_Room_OccupiedSlots", "OccupiedSlots <= BaseCapacity");
                    table.ForeignKey(
                        name: "FK_Rooms_BoardingHouses_BoardingHouseId",
                        column: x => x.BoardingHouseId,
                        principalTable: "BoardingHouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BoardingHouseId_RoomNumber",
                table: "Rooms",
                columns: new[] { "BoardingHouseId", "RoomNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
