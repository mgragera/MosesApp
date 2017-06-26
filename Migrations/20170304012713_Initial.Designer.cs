using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Moses.Models;

namespace Moses.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170304012713_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

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
        }
    }
}
