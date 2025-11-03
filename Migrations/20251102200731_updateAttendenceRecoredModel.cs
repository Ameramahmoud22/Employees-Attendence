using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employees_Attendence.Migrations
{
    /// <inheritdoc />
    public partial class updateAttendenceRecoredModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "TimeIn",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "TimeOut",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<bool>(
                name: "IsPresent",
                table: "AttendanceRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPresent",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeIn",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOut",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: true);
        }
    }
}
