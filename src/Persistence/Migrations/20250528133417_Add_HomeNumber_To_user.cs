using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_HomeNumber_To_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_claim");

            migrationBuilder.AddColumn<string>(
                name: "home_number",
                table: "user",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "home_number",
                table: "user");

            migrationBuilder.CreateTable(
                name: "user_claim",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<string>(type: "TEXT", nullable: false),
                    claim_type = table.Column<string>(type: "TEXT", nullable: false),
                    claim_value = table.Column<string>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    deleted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true),
                    last_modified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_modified_by = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_claim_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_user_claim_user_id",
                table: "user_claim",
                column: "user_id");
        }
    }
}
