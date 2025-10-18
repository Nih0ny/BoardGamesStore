using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoardGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                schema: "board_games_store",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SimilarProducts",
                schema: "board_games_store",
                table: "SimilarProducts");

            migrationBuilder.DropIndex(
                name: "IX_SimilarProducts_ProductId",
                schema: "board_games_store",
                table: "SimilarProducts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                schema: "board_games_store",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleId",
                schema: "board_games_store",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "board_games_store",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleName",
                schema: "board_games_store",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "board_games_store",
                table: "SimilarProducts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "board_games_store",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                schema: "board_games_store",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                schema: "board_games_store",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                schema: "board_games_store",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SimilarProducts",
                schema: "board_games_store",
                table: "SimilarProducts",
                columns: new[] { "ProductId", "SimilarProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                schema: "board_games_store",
                table: "Carts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SimilarProducts",
                schema: "board_games_store",
                table: "SimilarProducts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                schema: "board_games_store",
                table: "Carts");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "board_games_store",
                table: "SimilarProducts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "board_games_store",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                schema: "board_games_store",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                schema: "board_games_store",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                schema: "board_games_store",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                schema: "board_games_store",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                schema: "board_games_store",
                table: "AspNetRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SimilarProducts",
                schema: "board_games_store",
                table: "SimilarProducts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarProducts_ProductId",
                schema: "board_games_store",
                table: "SimilarProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                schema: "board_games_store",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                schema: "board_games_store",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                schema: "board_games_store",
                table: "AspNetUsers",
                column: "RoleId",
                principalSchema: "board_games_store",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
