using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStatsView : Migration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE MATERIALIZED VIEW product_stats_mv AS
            SELECT 
                p.""Id"" as ""ProductId"",
                COALESCE(AVG(e.""Rating""), 0) as ""AverageRating"",
                COUNT(e.""Id"") as ""ReviewsCount""
            FROM ""products"" p
            LEFT JOIN ""evaluations"" e ON p.""Id"" = e.""ProductId""
            GROUP BY p.""Id"";
        ");

            migrationBuilder.Sql(@"
            CREATE UNIQUE INDEX idx_product_stats_mv_id 
            ON product_stats_mv (""ProductId"");
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS product_stats_mv;");
        }
    }
}
