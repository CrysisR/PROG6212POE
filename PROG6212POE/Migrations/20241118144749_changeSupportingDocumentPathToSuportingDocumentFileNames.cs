using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG6212POE.Migrations
{
    /// <inheritdoc />
    public partial class changeSupportingDocumentPathToSuportingDocumentFileNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportingDocumentsPaths",
                table: "Claims",
                newName: "SupportingDocumentFileNames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportingDocumentFileNames",
                table: "Claims",
                newName: "SupportingDocumentsPaths");
        }
    }
}
