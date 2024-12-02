using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeskMotion.Migrations
{
    /// <inheritdoc />
    public partial class AddDeskPreferencesAndInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeskInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Floor = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    CurrentHeight = table.Column<double>(type: "double precision", nullable: false),
                    MinHeight = table.Column<int>(type: "integer", nullable: false),
                    MaxHeight = table.Column<int>(type: "integer", nullable: false),
                    IsMoving = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: true),
                    QRCodeData = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeskInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeskPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultHeight = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeskPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeskPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeskPreferences_DeskInfos_DeskId",
                        column: x => x.DeskId,
                        principalTable: "DeskInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeightAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeskId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldHeight = table.Column<double>(type: "double precision", nullable: false),
                    NewHeight = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeightAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeightAdjustments_DeskInfos_DeskId",
                        column: x => x.DeskId,
                        principalTable: "DeskInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledHeights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledHeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledHeights_DeskPreferences_PreferenceId",
                        column: x => x.PreferenceId,
                        principalTable: "DeskPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_DeskId",
                table: "Reservations",
                column: "DeskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeskPreferences_DeskId",
                table: "DeskPreferences",
                column: "DeskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeskPreferences_UserId",
                table: "DeskPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeightAdjustments_DeskId",
                table: "HeightAdjustments",
                column: "DeskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledHeights_PreferenceId",
                table: "ScheduledHeights",
                column: "PreferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_DeskInfos_DeskId",
                table: "Reservations",
                column: "DeskId",
                principalTable: "DeskInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_DeskInfos_DeskId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "HeightAdjustments");

            migrationBuilder.DropTable(
                name: "ScheduledHeights");

            migrationBuilder.DropTable(
                name: "DeskPreferences");

            migrationBuilder.DropTable(
                name: "DeskInfos");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_DeskId",
                table: "Reservations");
        }
    }
}
