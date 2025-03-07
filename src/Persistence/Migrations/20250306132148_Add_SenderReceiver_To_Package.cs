using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_SenderReceiver_To_Package : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sender_id",
                table: "package",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "i_x_package_sender_id",
                table: "package",
                column: "sender_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_package__user_sender_id",
                table: "package",
                column: "sender_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_package__user_sender_id",
                table: "package");

            migrationBuilder.DropIndex(
                name: "i_x_package_sender_id",
                table: "package");

            migrationBuilder.DropColumn(
                name: "sender_id",
                table: "package");
        }
    }
}
