using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class InitPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "course",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailUrl",
                table: "course",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalePrice",
                table: "course",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderCode = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Method = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "course",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_payment_CourseId",
                table: "payment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_OrderCode",
                table: "payment",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_UserId",
                table: "payment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_course_category_category_CategoryId",
                table: "course_category",
                column: "CategoryId",
                principalTable: "category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_course_category_course_CourseId",
                table: "course_category",
                column: "CourseId",
                principalTable: "course",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_category_category_CategoryId",
                table: "course_category");

            migrationBuilder.DropForeignKey(
                name: "FK_course_category_course_CourseId",
                table: "course_category");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "course",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "course",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "course",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "course",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "course",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "SalePrice",
                table: "course",
                newName: "sale_price");

            migrationBuilder.RenameColumn(
                name: "ListedPrice",
                table: "course",
                newName: "listed_price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "category",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "category",
                newName: "id");

            migrationBuilder.UpdateData(
                table: "course",
                keyColumn: "description",
                keyValue: null,
                column: "description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "course",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "course",
                keyColumn: "thumbnail_url",
                keyValue: null,
                column: "thumbnail_url",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "thumbnail_url",
                table: "course",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "sale_price",
                table: "course",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_course_category_category_category_id",
                table: "course_category",
                column: "CategoryId",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_course_category_course_course_id",
                table: "course_category",
                column: "CourseId",
                principalTable: "course",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
