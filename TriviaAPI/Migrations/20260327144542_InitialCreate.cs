using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TriviaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuizId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    MediaUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MediaType = table.Column<string>(type: "TEXT", nullable: true),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnswerText = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "1960s Music", new DateTime(2026, 3, 27, 14, 45, 41, 681, DateTimeKind.Utc).AddTicks(9250), true, "Classic Hits from the 60s" },
                    { 2, "1970s Movies", new DateTime(2026, 3, 27, 14, 45, 41, 682, DateTimeKind.Utc).AddTicks(1180), true, "Iconic Films of the 70s" },
                    { 3, "1950s History", new DateTime(2026, 3, 27, 14, 45, 41, 682, DateTimeKind.Utc).AddTicks(1190), true, "Historical Events of the 50s" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 27, 14, 45, 41, 241, DateTimeKind.Utc).AddTicks(3640), "admin@hotmail.com", "$2a$11$Zhx3M4KD/yvOjZABRkggKOnJcBON3VvNXuAy9EvOldGFKdpNRqXGS", "Admin" },
                    { 2, new DateTime(2026, 3, 27, 14, 45, 41, 540, DateTimeKind.Utc).AddTicks(5590), "user@hotmail.com", "$2a$11$/cEMC7V9yXVYOrPIGwQpwO2vH0tkVyFYgPX/llSJR5vYQyepYTiVG", "User" }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "MediaType", "MediaUrl", "OrderIndex", "QuestionText", "QuizId" },
                values: new object[,]
                {
                    { 1, "audio", "/media/imagine.mp3", 1, "Who sang 'Imagine'?", 1 },
                    { 2, "image", "/media/beatles.jpg", 2, "Which band is shown in this photo?", 1 },
                    { 3, null, null, 3, "Who sang 'Respect'?", 1 },
                    { 4, "video", "/media/godfather.mp4", 1, "Name this classic movie scene", 2 },
                    { 5, "image", "/media/deniro.jpg", 2, "Who is this legendary actor?", 2 },
                    { 6, null, null, 3, "Complete the quote: 'You talkin' to me?'", 2 },
                    { 7, "image", "/media/1950s-event.jpg", 1, "What year did this event occur?", 3 },
                    { 8, null, null, 2, "Who was US President in 1953?", 3 },
                    { 9, "audio", "/media/speech.mp3", 3, "Identify this famous speech", 3 }
                });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "AnswerText", "IsCorrect", "OrderIndex", "QuestionId" },
                values: new object[,]
                {
                    { 1, "John Lennon", true, 1, 1 },
                    { 2, "Paul McCartney", false, 2, 1 },
                    { 3, "Bob Dylan", false, 3, 1 },
                    { 4, "The Beatles", true, 1, 2 },
                    { 5, "The Rolling Stones", false, 2, 2 },
                    { 6, "The Who", false, 3, 2 },
                    { 7, "Aretha Franklin", true, 1, 3 },
                    { 8, "Diana Ross", false, 2, 3 },
                    { 9, "Etta James", false, 3, 3 },
                    { 10, "The Godfather", true, 1, 4 },
                    { 11, "Scarface", false, 2, 4 },
                    { 12, "Goodfellas", false, 3, 4 },
                    { 13, "Robert De Niro", true, 1, 5 },
                    { 14, "Al Pacino", false, 2, 5 },
                    { 15, "Marlon Brando", false, 3, 5 },
                    { 16, "Taxi Driver", true, 1, 6 },
                    { 17, "Raging Bull", false, 2, 6 },
                    { 18, "The Deer Hunter", false, 3, 6 },
                    { 19, "1955", true, 1, 7 },
                    { 20, "1952", false, 2, 7 },
                    { 21, "1958", false, 3, 7 },
                    { 22, "Dwight D. Eisenhower", true, 1, 8 },
                    { 23, "Harry S. Truman", false, 2, 8 },
                    { 24, "John F. Kennedy", false, 3, 8 },
                    { 25, "Eisenhower's Farewell", true, 1, 9 },
                    { 26, "Churchill's Iron Curtain", false, 2, 9 },
                    { 27, "Truman Doctrine", false, 3, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId",
                table: "Questions",
                column: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Quizzes");
        }
    }
}
