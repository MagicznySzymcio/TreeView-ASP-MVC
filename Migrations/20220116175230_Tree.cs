using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeView_ASP_MVC.Migrations
{
    public partial class Tree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 1, 1, "Program Files", null });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 2, 1, "Programy", null });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 3, 1, "Projekty", null });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[,]
                {
                    { 4, 2, "Adobe", 1 },
                    { 5, 2, "Microsoft", 1 },
                    { 9, 2, "MPC HC", 2 },
                    { 10, 2, "HoneyView", 2 }
                });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 6, 3, "Windows Defender", 5 });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 7, 3, "Azure", 5 });

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Depth", "Name", "ParentId" },
                values: new object[] { 8, 3, "Temp", 5 });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ParentId",
                table: "Nodes",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
