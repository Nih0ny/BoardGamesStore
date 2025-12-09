using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BoardGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "embedding",
                table: "products",
                type: "vector(384)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "embedding",
                table: "products");
        }
    }
}
