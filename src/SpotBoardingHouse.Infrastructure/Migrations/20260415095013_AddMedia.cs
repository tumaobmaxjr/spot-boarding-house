using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotBoardingHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardingHouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.CheckConstraint("CK_Media_TargetXor", "(([BoardingHouseId] IS NOT NULL AND [RoomId] IS NULL) OR ([BoardingHouseId] IS NULL AND [RoomId] IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_Media_BoardingHouses_BoardingHouseId",
                        column: x => x.BoardingHouseId,
                        principalTable: "BoardingHouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Media_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_BoardingHouseId",
                table: "Media",
                column: "BoardingHouseId",
                unique: true,
                filter: "[BoardingHouseId] IS NOT NULL AND [IsPrimary] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Media_RoomId",
                table: "Media",
                column: "RoomId",
                unique: true,
                filter: "[RoomId] IS NOT NULL AND [IsPrimary] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media");
        }
    }
}
