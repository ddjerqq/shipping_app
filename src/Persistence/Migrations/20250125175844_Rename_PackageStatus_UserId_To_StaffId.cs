using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Rename_PackageStatus_UserId_To_StaffId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_package_reception_status_user_user_id",
                table: "package_reception_status");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "package_reception_status",
                newName: "staff_id");

            migrationBuilder.RenameIndex(
                name: "i_x_package_reception_status_user_id",
                table: "package_reception_status",
                newName: "i_x_package_reception_status_staff_id");

            migrationBuilder.AlterColumn<string>(
                name: "website_address",
                table: "package",
                type: "TEXT",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "i_x_race_name",
                table: "race",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_package_reception_status_user_staff_id",
                table: "package_reception_status",
                column: "staff_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_package_reception_status_user_staff_id",
                table: "package_reception_status");

            migrationBuilder.DropIndex(
                name: "i_x_race_name",
                table: "race");

            migrationBuilder.RenameColumn(
                name: "staff_id",
                table: "package_reception_status",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "i_x_package_reception_status_staff_id",
                table: "package_reception_status",
                newName: "i_x_package_reception_status_user_id");

            migrationBuilder.AlterColumn<string>(
                name: "website_address",
                table: "package",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_package_reception_status_user_user_id",
                table: "package_reception_status",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
