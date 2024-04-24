using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonInterestsApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonEmail",
                table: "Persons",
                newName: "PersonPhone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonPhone",
                table: "Persons",
                newName: "PersonEmail");
        }
    }
}
