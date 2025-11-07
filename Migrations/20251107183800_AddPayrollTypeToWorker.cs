using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employees_Attendence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollTypeToWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayrollType",
                table: "Workers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayrollType",
                table: "Workers");
        }
    }
}
