using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prober.Migrations
{
    /// <inheritdoc />
    public partial class InitSlaDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Indicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Slo = table.Column<double>(type: "double", nullable: false),
                    Value = table.Column<double>(type: "double", nullable: false),
                    IsBad = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicators", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Indicators_Name",
                table: "Indicators",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Indicators_Time",
                table: "Indicators",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Indicators");
        }
    }
}
