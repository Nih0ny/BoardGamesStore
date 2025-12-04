using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoardGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_items_carts_CartId",
                table: "cart_items");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropIndex(
                name: "IX_cart_items_CartId",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "order_statuses",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusTotal",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "cart_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_UserId",
                table: "cart_items",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_items_AspNetUsers_UserId",
                table: "cart_items",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_items_AspNetUsers_UserId",
                table: "cart_items");

            migrationBuilder.DropIndex(
                name: "IX_cart_items_UserId",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "products");

            migrationBuilder.DropColumn(
                name: "BonusTotal",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "order_statuses",
                newName: "Status");

            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "cart_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_carts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_CartId",
                table: "cart_items",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_carts_UserId",
                table: "carts",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cart_items_carts_CartId",
                table: "cart_items",
                column: "CartId",
                principalTable: "carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
