using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.db.migrations
{
    /// <inheritdoc />
    public partial class makehomenumberanencryptedqueryable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "home_number_shadow_hash",
                table: "user",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "home_number_shadow_hash",
                table: "user");
        }
    }
}
