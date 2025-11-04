using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employees_Attendence.Migrations
{
    /// <inheritdoc />
    public partial class updateworkersModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Advance",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "Deductions",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "IsPresent",
                table: "AttendanceRecords");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AttendanceRecords",
                newName: "AttendanceDate");

            migrationBuilder.AddColumn<decimal>(
                name: "Advance",
                table: "WeeklyPayrollRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Advance",
                table: "WeeklyPayrollRecords");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AttendanceRecords");

            migrationBuilder.RenameColumn(
                name: "AttendanceDate",
                table: "AttendanceRecords",
                newName: "Date");

            migrationBuilder.AddColumn<decimal>(
                name: "Advance",
                table: "Workers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Deductions",
                table: "Workers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPresent",
                table: "AttendanceRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
