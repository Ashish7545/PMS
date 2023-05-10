using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    public partial class Update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EmployeeID",
                table: "Projects",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Employees_EmployeeID",
                table: "Projects",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employees_EmployeeID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_EmployeeID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "Projects");
        }
    }
}
