using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Moses.Models;

namespace Moses.Migrations
{
    public partial class initialcomplete : Migration
    {
        private ApplicationDbContext ctx = new ApplicationDbContext();
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Glosarios",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CodLenguaje = table.Column<string>(nullable: true),
                    Palabra = table.Column<string>(nullable: true),
                    Grupo = table.Column<int>(nullable:true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glosarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lenguajes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CodLenguaje_destino = table.Column<string>(nullable: true),
                    CodLenguaje_origen = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lenguajes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Actualizado = table.Column<bool>(nullable: false),
                    Descripcion = table.Column<string>(nullable: true),
                    Fecha_modificacion = table.Column<string>(nullable: true),
                    // GlosarioIdId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                    /*table.ForeignKey(
                        name: "FK_Proyectos_Glosarios_GlosarioIdId",
                        column: x => x.GlosarioIdId,
                        principalTable: "Glosarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);*/
                });

            migrationBuilder.CreateTable(
                name: "Memorias",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Fecha_modificacion = table.Column<string>(nullable: true),
                    LenguajeIdId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memorias_Lenguajes_LenguajeIdId",
                        column: x => x.LenguajeIdId,
                        principalTable: "Lenguajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proyecto_Memorias",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    MemoriaIdId = table.Column<int>(nullable: true),
                    ProyectoIdId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyecto_Memorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proyecto_Memorias_Memorias_MemoriaIdId",
                        column: x => x.MemoriaIdId,
                        principalTable: "Memorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proyecto_Memorias_Proyectos_ProyectoIdId",
                        column: x => x.ProyectoIdId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proyecto_Glosarios",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    GlosarioIdId = table.Column<int>(nullable: true),
                    ProyectoIdId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyecto_Glosarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proyecto_Glosarios_Glosarios_GlosarioIdId",
                        column: x => x.GlosarioIdId,
                        principalTable: "Glosarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proyecto_Glosarios_Proyectos_ProyectoIdId",
                        column: x => x.ProyectoIdId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateIndex(
                name: "IX_Memorias_LenguajeIdId",
                table: "Memorias",
                column: "LenguajeIdId");

            /*migrationBuilder.CreateIndex(
                name: "IX_Proyectos_GlosarioIdId",
                table: "Proyectos",
                column: "GlosarioIdId");*/

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_Memorias_MemoriaIdId",
                table: "Proyecto_Memorias",
                column: "MemoriaIdId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_Memorias_ProyectoIdId",
                table: "Proyecto_Memorias",
                column: "ProyectoIdId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_Glosarios_GlosarioIdId",
                table: "Proyecto_Glosarios",
                column: "GlosarioIdId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_Glosarios_ProyectoIdId",
                table: "Proyecto_Glosarios",
                column: "ProyectoIdId");

            var usuario = new Usuario(){
                Tipo = 1,
                CodUsuario = "admin",
                Contrasena = "admin1234"
            };
            ctx.Usuarios.Add(usuario);
            usuario = new Usuario(){
                Tipo = 2,
                CodUsuario = "traductor",
                Contrasena = "traductor1234"
            };
            ctx.Usuarios.Add(usuario);

            ctx.SaveChanges();


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proyecto_Memorias");

            migrationBuilder.DropTable(
                name: "Memorias");

            migrationBuilder.DropTable(
                name: "Proyectos");

            migrationBuilder.DropTable(
                name: "Lenguajes");

            /*migrationBuilder.DropTable(
                name: "Glosarios");*/
        }
    }
}
