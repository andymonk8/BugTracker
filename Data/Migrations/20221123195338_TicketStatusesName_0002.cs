using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    public partial class TicketStatusesName_0002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketStauses_TicketStatusId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketStauses",
                table: "TicketStauses");

            migrationBuilder.RenameTable(
                name: "TicketStauses",
                newName: "TicketStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketStatuses",
                table: "TicketStatuses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketStatuses_TicketStatusId",
                table: "Tickets",
                column: "TicketStatusId",
                principalTable: "TicketStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketStatuses_TicketStatusId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketStatuses",
                table: "TicketStatuses");

            migrationBuilder.RenameTable(
                name: "TicketStatuses",
                newName: "TicketStauses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketStauses",
                table: "TicketStauses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketStauses_TicketStatusId",
                table: "Tickets",
                column: "TicketStatusId",
                principalTable: "TicketStauses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
