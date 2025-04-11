using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "Rewards",
                newName: "Points");

            migrationBuilder.CreateIndex(
                name: "IX_ChildRewards_RewardId",
                table: "ChildRewards",
                column: "RewardId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildRewards_Children_ChildId",
                table: "ChildRewards",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildRewards_Rewards_RewardId",
                table: "ChildRewards",
                column: "RewardId",
                principalTable: "Rewards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildRewards_Children_ChildId",
                table: "ChildRewards");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildRewards_Rewards_RewardId",
                table: "ChildRewards");

            migrationBuilder.DropIndex(
                name: "IX_ChildRewards_RewardId",
                table: "ChildRewards");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Rewards",
                newName: "Cost");
        }
    }
}
