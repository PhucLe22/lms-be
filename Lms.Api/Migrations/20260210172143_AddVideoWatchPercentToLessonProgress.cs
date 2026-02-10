using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lms.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoWatchPercentToLessonProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoWatchPercent",
                table: "LessonProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoWatchPercent",
                table: "LessonProgresses");
        }
    }
}
