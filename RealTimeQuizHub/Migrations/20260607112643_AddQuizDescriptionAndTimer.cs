using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTimeQuizHub.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizDescriptionAndTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quizzes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasTimer",
                table: "Quizzes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TimerScoreImpact",
                table: "Quizzes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TimerSecondsPerQuestion",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "HasTimer",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "TimerScoreImpact",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "TimerSecondsPerQuestion",
                table: "Quizzes");
        }
    }
}
