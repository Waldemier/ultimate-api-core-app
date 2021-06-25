using Microsoft.EntityFrameworkCore.Migrations;

namespace UltimateWebApi.Migrations
{
    public partial class SeedRolesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "738db78f-6ea5-45d8-82de-4f92c85a1e7d", "66ee34df-cb7c-443d-bfd7-b93de514bd99", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "e8ecad93-129f-4f69-8e2f-b6ea4770cb0a", "f2f9d2fe-3c3f-4c8c-a306-53c19d43ee1a", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "738db78f-6ea5-45d8-82de-4f92c85a1e7d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8ecad93-129f-4f69-8e2f-b6ea4770cb0a");
        }
    }
}
