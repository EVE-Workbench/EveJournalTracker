using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class nullableevesystemfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "SecurityStatus",
                table: "EveSystems",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityClass",
                table: "EveSystems",
                type: "TEXT",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "SecurityStatus",
                table: "EveSystems",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SecurityClass",
                table: "EveSystems",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
