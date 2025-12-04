#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CafeAvalonia.Migrations
{
    public partial class Initial_Clean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Order",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Order");
        }
    }
}
