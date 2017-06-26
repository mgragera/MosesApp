using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Moses.Models;

namespace Moses.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170319013845_initialcomplete")]
    partial class initialcomplete
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("Moses.Models.Glosario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Palabra_antigua");

                    b.Property<string>("Palabra_nueva");

                    b.HasKey("Id");

                    b.ToTable("Glosarios");
                });

            modelBuilder.Entity("Moses.Models.Lenguaje", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CodLenguaje_destino");

                    b.Property<string>("CodLenguaje_origen");

                    b.HasKey("Id");

                    b.ToTable("Lenguajes");
                });

            modelBuilder.Entity("Moses.Models.Memoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Fecha_modificacion");

                    b.Property<int?>("LenguajeIdId");

                    b.HasKey("Id");

                    b.HasIndex("LenguajeIdId");

                    b.ToTable("Memorias");
                });

            modelBuilder.Entity("Moses.Models.Proyecto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Actualizado");

                    b.Property<string>("Descripcion");

                    b.Property<string>("Fecha_modificacion");

                    b.Property<int?>("GlosarioIdId");

                    b.HasKey("Id");

                    b.HasIndex("GlosarioIdId");

                    b.ToTable("Proyectos");
                });

            modelBuilder.Entity("Moses.Models.Proyecto_Memoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("MemoriaIdId");

                    b.Property<int?>("ProyectoIdId");

                    b.HasKey("Id");

                    b.HasIndex("MemoriaIdId");

                    b.HasIndex("ProyectoIdId");

                    b.ToTable("Proyecto_Memorias");
                });

            modelBuilder.Entity("Moses.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CodUsuario");

                    b.Property<string>("Contrasena");

                    b.Property<int>("Tipo");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("Moses.Models.Memoria", b =>
                {
                    b.HasOne("Moses.Models.Lenguaje", "LenguajeId")
                        .WithMany()
                        .HasForeignKey("LenguajeIdId");
                });

            modelBuilder.Entity("Moses.Models.Proyecto", b =>
                {
                    b.HasOne("Moses.Models.Glosario", "GlosarioId")
                        .WithMany()
                        .HasForeignKey("GlosarioIdId");
                });

            modelBuilder.Entity("Moses.Models.Proyecto_Memoria", b =>
                {
                    b.HasOne("Moses.Models.Memoria", "MemoriaId")
                        .WithMany()
                        .HasForeignKey("MemoriaIdId");

                    b.HasOne("Moses.Models.Proyecto", "ProyectoId")
                        .WithMany()
                        .HasForeignKey("ProyectoIdId");
                });
        }
    }
}
