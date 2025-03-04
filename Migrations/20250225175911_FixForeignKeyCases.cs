using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Migrations
{
    public partial class FixForeignKeyCases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure the Status and Contact columns are altered as needed
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Contact",
                table: "Cases",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // Add the foreign key without renaming the column
            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_EmpID",  // Use the existing EmpID column
                table: "Cases",
                column: "EmpID",  // Use EmpID instead of CreatedBy
                principalTable: "Users",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Skip dropping the non-existent constraint
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Cases_Users_CreatedBy",
            //    table: "Cases");

            // Revert the column alterations if needed
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Contact",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

    }
}
