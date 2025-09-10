using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorTemplate.Database.Migrations
{
    /// <inheritdoc />
    public partial class SwitchFromSendGridToSmtp2Go : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendGridSystemEmailAddress",
                table: "SystemSettings",
                newName: "SystemEmailAddress");

            migrationBuilder.RenameColumn(
                name: "SendGridKey",
                table: "SystemSettings",
                newName: "EmailApiKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SystemEmailAddress",
                table: "SystemSettings",
                newName: "SendGridSystemEmailAddress");

            migrationBuilder.RenameColumn(
                name: "EmailApiKey",
                table: "SystemSettings",
                newName: "SendGridKey");
        }
    }
}
