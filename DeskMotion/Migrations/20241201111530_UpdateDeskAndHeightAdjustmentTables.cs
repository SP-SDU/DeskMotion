using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeskMotion.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeskAndHeightAdjustmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing HeightAdjustments table
            migrationBuilder.DropTable(
                name: "HeightAdjustments");

            // Create new HeightAdjustments table
            migrationBuilder.CreateTable(
                name: "HeightAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_HeightAdjustments_DeskId",
                table: "HeightAdjustments",
                column: "DeskId");

            migrationBuilder.AlterColumn<double>(
                name: "MinHeight",
                table: "DeskInfos",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "MaxHeight",
                table: "DeskInfos",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "CurrentHeight",
                table: "DeskInfos",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "DeskInfos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedUserName",
                table: "DeskInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentEnd",
                table: "DeskInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentStart",
                table: "DeskInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DeskInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                table: "DeskInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemporaryAssignment",
                table: "DeskInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeightAdjustments");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "AssignedUserName",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "AssignmentEnd",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "AssignmentStart",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "IsAssigned",
                table: "DeskInfos");

            migrationBuilder.DropColumn(
                name: "IsTemporaryAssignment",
                table: "DeskInfos");

            migrationBuilder.AlterColumn<int>(
                name: "MinHeight",
                table: "DeskInfos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "MaxHeight",
                table: "DeskInfos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "CurrentHeight",
                table: "DeskInfos",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            // Recreate original HeightAdjustments table
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

            migrationBuilder.CreateIndex(
                name: "IX_HeightAdjustments_DeskId",
                table: "HeightAdjustments",
                column: "DeskId");
        }
    }
}
