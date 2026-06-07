using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RealTimeQuizHub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoomsQuizScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserScores_Rooms_RoomId",
                table: "UserScores");

            // NOTE: the `rooms` table is intentionally left in place (kept untouched
            // in the database) — it is simply no longer mapped or used by the app.

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "UserScores",
                newName: "QuizId");

            migrationBuilder.RenameIndex(
                name: "IX_UserScores_UserId_RoomId",
                table: "UserScores",
                newName: "IX_UserScores_UserId_QuizId");

            migrationBuilder.RenameIndex(
                name: "IX_UserScores_RoomId",
                table: "UserScores",
                newName: "IX_UserScores_QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserScores_Quizzes_QuizId",
                table: "UserScores",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserScores_Quizzes_QuizId",
                table: "UserScores");

            migrationBuilder.RenameColumn(
                name: "QuizId",
                table: "UserScores",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_UserScores_UserId_QuizId",
                table: "UserScores",
                newName: "IX_UserScores_UserId_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_UserScores_QuizId",
                table: "UserScores",
                newName: "IX_UserScores_RoomId");

            // The `rooms` table was left untouched by Up, so it still exists here —
            // only re-point the UserScores FK back to it.
            migrationBuilder.AddForeignKey(
                name: "FK_UserScores_Rooms_RoomId",
                table: "UserScores",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
